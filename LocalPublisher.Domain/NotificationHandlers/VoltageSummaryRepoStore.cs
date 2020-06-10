using Entities;
using LazyCache;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ReadRepository.Cosmos;
using ReadRepository.Repositories;
using Repository;
using Serilog;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageSummaryRepoStore :
        INotificationHandler<VoltageSummaryRepoStore.PersistenceInfo>, // TOTO this should be a command, not publish
        INotificationHandler<VoltageSummary>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IAppCache _cache;
        private readonly IVoltageSummaryRepository _voltageSummaryRepository;
        private readonly ISenecCompressConfig _config;
        private readonly IApplicationVersion _versionConfig;
        private readonly IVoltageSummaryDocumentReadRepository _voltageSummaryReadRepository;

        public class PersistenceInfo : INotification
        {
            private int _retryCount;
            public VoltageSummary Summary { get; set; }
            public bool IsProcessing { get; set; }

            public int GetRetryCount()
            {
                return _retryCount;
            }

            public void Increment()
            {
                _retryCount++;
            }
        }

        public VoltageSummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            IAppCache cache,
            ISenecCompressConfig config,
            IApplicationVersion versionConfig,
            IVoltageSummaryDocumentReadRepository voltageSummaryReadRepository,
            IVoltageSummaryRepository voltageSummaryRepository)
        {
            _logger = logger;
            _mediator = mediator;
            _cache = cache;
            _config = config;
            _versionConfig = versionConfig;
            _voltageSummaryReadRepository = voltageSummaryReadRepository;
            _voltageSummaryRepository = voltageSummaryRepository;
        }

        public async Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            var inMemoryCollection = GetCollection();
            var key = notification.GetKey();
            var added = inMemoryCollection.TryAdd(key, new PersistenceInfo { Summary = notification });
            if (!added)
            {
                _logger.Error("Summary {Key} already added to collection", key);
            }
            await PersistAsync(key, cancellationToken);
            await PublishWaitingSummariesAsync(cancellationToken);
        }

        public async Task Handle(PersistenceInfo notification, CancellationToken cancellationToken)
        {
            var summary = notification.Summary;
            var verifyResult = await VerifyAsync(summary, cancellationToken);
            if (!verifyResult.isPersisted)
            {
                notification.Increment();
                var delay = TimeSpan.FromSeconds(Math.Min((int)Math.Pow(2, notification.GetRetryCount()), 60));
                _logger.Error("Waiting {Seconds} before publishing {@Summary}", delay.TotalSeconds, summary);
                await Task.Delay(delay, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
                await PersistAsync(summary.GetKey(), cancellationToken);
            }
            else
            {
                _logger.Warning("Rehandled voltage summary has been persisted {Key} and removed {Removed}", notification.Summary.GetKey(), verifyResult.isRemoved);
            }
        }

        private ConcurrentDictionary<string, PersistenceInfo> GetCollection()
        {
            return _cache.GetOrAdd("persistVoltageSummaryList", () => new ConcurrentDictionary<string, PersistenceInfo>());
        }

        private async Task PersistAsync(string key, CancellationToken cancellationToken)
        {
            var inMemoryCollection = GetCollection();
            if (!inMemoryCollection.TryGetValue(key, out PersistenceInfo item))
                return;
            try
            {
                item.IsProcessing = true;
                await FetchPersistedVersionAsync(item.Summary, cancellationToken);

                if (_versionConfig.PersistedNumber == _versionConfig.Number)
                    await WriteAndVerifyAsync(item.Summary, cancellationToken);
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                    if (!await IsPersisted(item.Summary, cancellationToken))
                    {
                        await WriteAndVerifyAsync(item.Summary, cancellationToken);
                        _versionConfig.PersistedNumber = _versionConfig.Number;
                    }
                }
            }
            catch (Microsoft.Azure.Cosmos.CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.Warning("Conflicted volt summary {Key}", key);
                if ((await VerifyAsync(item.Summary, cancellationToken)).isPersisted)
                    return;
                else
                    await PublishWaitingSummariesAsync(cancellationToken);

            }
            catch (Exception e)
            {
                _logger.Error(e, "Voltage summary persistence failure");
                var cosmosEx = e as Microsoft.Azure.Cosmos.CosmosException;
                await PublishWaitingSummariesAsync(cancellationToken);
            }
            finally
            {
                item.IsProcessing = false;
            }
        }

        private async Task PublishWaitingSummariesAsync(CancellationToken cancellationToken)
        {
            var inMemoryCollection = GetCollection();
            var inMemoryKey = inMemoryCollection.Keys.FirstOrDefault();
            PersistenceInfo summary;
            if (inMemoryKey != null)
                if (inMemoryCollection.TryGetValue(inMemoryKey, out summary))
                    if (!summary.IsProcessing)
                        await _mediator.Publish(summary, cancellationToken);
        }

        private async Task WriteAndVerifyAsync(VoltageSummary notification, CancellationToken cancellationToken)
        {
            bool isPersisted = false, isRemoved = false;
            var written = await WriteAsync(notification, cancellationToken);
            if (written)
            {
                (isPersisted, isRemoved) = await VerifyAsync(notification, cancellationToken);
            }
            var logMethod = isPersisted ? (Action<string, object[]>)_logger.Information : _logger.Error;
            logMethod("Voltage summary {Summary} Written {Written} Persisted {isPersisted} and removed {Removed}", new object[] { notification, written, isPersisted, isRemoved });
        }

        private async Task<(bool isPersisted, bool isRemoved)> VerifyAsync(VoltageSummary summary, CancellationToken cancellationToken)
        {
            bool isRemoved = false;
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            var isPersisted = await IsPersisted(summary, cancellationToken);
            if (isPersisted)
            {
                var inMemoryCollection = GetCollection();
                isRemoved = inMemoryCollection.TryRemove(summary.GetKey(), out PersistenceInfo _);
            }
            return (isPersisted, isRemoved);
        }

        private async Task<bool> IsPersisted(VoltageSummary notification, CancellationToken cancellationToken)
        {
            var result = await _voltageSummaryReadRepository.Get(notification.GetKey(), cancellationToken);
            return result != null;
        }

        private async Task<bool> WriteAsync(VoltageSummary notification, CancellationToken cancellationToken)
        {
            _logger.Information("Writing {Time}", notification.GetKey());
            var entriesWritten = await _voltageSummaryRepository.AddAsync(notification, cancellationToken);
            _logger.Information("Written {Time} {Entries}", notification.GetKey(), entriesWritten);
            return entriesWritten > 0;
        }

        private async Task FetchPersistedVersionAsync(VoltageSummary summary, CancellationToken cancellationToken)
        {
            if (_versionConfig.PersistedNumber != null) return;

            var previousIntervalStart = summary.IntervalStartIncluded - (summary.IntervalEndExcluded - summary.IntervalStartIncluded);
            var persistedRecord =
                await _voltageSummaryReadRepository.Get(previousIntervalStart.GetIntervalKey(), cancellationToken)
                ?? await _voltageSummaryReadRepository.Get(summary.GetKey(), cancellationToken);
            if (persistedRecord == null)
            {
                _versionConfig.PersistedNumber = 0;
            }
            else
            {
                _versionConfig.PersistedNumber = persistedRecord.Version;
            }
        }
    }
}
