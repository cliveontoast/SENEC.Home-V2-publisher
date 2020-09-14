using Entities;
using LazyCache;
using MediatR;
using Newtonsoft.Json;
using ReadRepository.Cosmos;
using Repository;
using Repository.Cosmos.Repositories;
using Serilog;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class EnergySummaryRepoStore :
        INotificationHandler<EnergySummaryRepoStore.PersistenceInfo>, // TODO this should be a command, not publish
        INotificationHandler<EnergySummary>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IAppCache _cache;
        private readonly IEnergySummaryRepository _EnergySummaryRepository;
        private readonly ISenecVoltCompressConfig _config;
        private readonly IApplicationVersion _versionConfig;
        private readonly IEnergySummaryDocumentReadRepository _EnergySummaryReadRepository;

        public class PersistenceInfo : INotification
        {
            private int _retryCount;
            public EnergySummary Summary { get; set; }

            public PersistenceInfo(EnergySummary summary)
            {
                Summary = summary;
            }

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

        public EnergySummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            IAppCache cache,
            ISenecVoltCompressConfig config,
            IApplicationVersion versionConfig,
            IEnergySummaryDocumentReadRepository EnergySummaryReadRepository,
            IEnergySummaryRepository EnergySummaryRepository)
        {
            _logger = logger;
            _mediator = mediator;
            _cache = cache;
            _config = config;
            _versionConfig = versionConfig;
            _EnergySummaryReadRepository = EnergySummaryReadRepository;
            _EnergySummaryRepository = EnergySummaryRepository;
        }

        public async Task Handle(EnergySummary notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Verbose("Handling {EnergySummary}", JsonConvert.SerializeObject(notification));
                var inMemoryCollection = GetCollection();
                var key = notification.GetKey();
                var added = inMemoryCollection.TryAdd(key, new PersistenceInfo(notification));
                if (!added)
                {
                    _logger.Error("Summary {Key} already added to collection", key);
                }
                await PersistAsync(key, cancellationToken);
                await PublishWaitingSummariesAsync(cancellationToken);
                _logger.Verbose("Handled {EnergySummary}", notification.GetKey());
            }
            catch (Exception e)
            {
                _logger.Verbose(e, "This shouldn't be necessary, something higher should capture and log this message");
                throw;
            }
        }

        public async Task Handle(PersistenceInfo notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Verbose("Persistence error. Handling energy summary {EnergySummary}", JsonConvert.SerializeObject(notification));
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
                    _logger.Warning("Rehandled energy summary has been persisted {Key} and removed {Removed}", notification.Summary.GetKey(), verifyResult.isRemoved);
                }
                _logger.Verbose("Persistence error. Handled energy summary {EnergySummary}", notification.Summary.GetKey());
            }
            catch (Exception e)
            {
                _logger.Verbose(e, "This shouldn't be necessary, something else should capture and log this message");
                throw;
            }
        }

        private ConcurrentDictionary<string, PersistenceInfo> GetCollection()
        {
            return _cache.GetOrAdd("persistEnergySummaryList", () => new ConcurrentDictionary<string, PersistenceInfo>(), DateTimeOffset.MaxValue);
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

                if (_versionConfig.PersistedNumber == _versionConfig.Number && _versionConfig.ThisProcessWrittenRecord)
                    await WriteAndVerifyAsync(item.Summary, cancellationToken);
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                    var verifyResult = await VerifyAsync(item.Summary, cancellationToken);
                    if (verifyResult.isPersisted)
                    {
                        await FetchPersistedVersionAsync(item.Summary, cancellationToken);
                        if (_versionConfig.PersistedNumber == _versionConfig.Number)
                        {
                            _logger.Warning("Persisted number is equal, however another process has persisted this record {Key}. Item removed {Removed}", key, verifyResult.isRemoved);
                        }
                        else
                        {
                            _logger.Information("Previous version is running and has persisted this record {Key}. Item removed {Removed}", key, verifyResult.isRemoved);
                        }
                    }
                    else
                    {
                        _logger.Information("This record {Key} has not been persisted. Attempting to write.", key, verifyResult.isRemoved);
                        await WriteAndVerifyAsync(item.Summary, cancellationToken);
                    }
                }
            }
            catch (Microsoft.Azure.Cosmos.CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.Warning("Conflicted volt summary {Key}", key);
                await VerifyAsync(item.Summary, cancellationToken);
            }
            // timeout 408 received
            catch (Exception e)
            {
                _logger.Error(e, "energy summary persistence failure");
            }
            finally
            {
                item.IsProcessing = false;
            }
        }

        private async Task PublishWaitingSummariesAsync(CancellationToken cancellationToken)
        {
            var inMemoryCollection = GetCollection();
            _logger.Verbose("Inmemory energy summary count {Count}", inMemoryCollection.Keys.Count);
            var inMemoryKey = inMemoryCollection.Keys.FirstOrDefault();
            PersistenceInfo summary;
            if (inMemoryKey != null)
                if (inMemoryCollection.TryGetValue(inMemoryKey, out summary))
                {
                    _logger.Information("Found an unprocessed count {Count} for item {Item}", summary.GetRetryCount(), JsonConvert.SerializeObject(summary));
                    if (!summary.IsProcessing)
                        await _mediator.Publish(summary, cancellationToken);
                }
        }

        private async Task WriteAndVerifyAsync(EnergySummary notification, CancellationToken cancellationToken)
        {
            bool isPersisted = false, isRemoved = false;
            var written = await WriteAsync(notification, cancellationToken);
            if (written)
            {
                (isPersisted, isRemoved) = await VerifyAsync(notification, cancellationToken);
                if (written && isPersisted)
                {
                    _versionConfig.ThisProcessWrittenRecord = true;
                    _logger.Information("This process written the record {Key}", notification.GetKey());
                }
            }
            var logMethod = isPersisted ? (Action<string, object[]>)_logger.Information : _logger.Error;
            logMethod("Written {Written} Persisted {isPersisted} Removed {Removed} energy summary {Summary} ", new object[] { written, isPersisted, isRemoved,
                Newtonsoft.Json.JsonConvert.SerializeObject(notification) });
        }

        private async Task<(bool isPersisted, bool isRemoved)> VerifyAsync(EnergySummary summary, CancellationToken cancellationToken)
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

        private async Task<bool> IsPersisted(EnergySummary notification, CancellationToken cancellationToken)
        {
            var result = await _EnergySummaryReadRepository.Get(notification.GetKey(), cancellationToken);
            return result != null;
        }

        private async Task<bool> WriteAsync(EnergySummary notification, CancellationToken cancellationToken)
        {
            _logger.Information("Writing {Time}", notification.GetKey());
            var entriesWritten = await _EnergySummaryRepository.AddAsync(notification, cancellationToken);
            _logger.Information("Written {Time} {Entries}", notification.GetKey(), entriesWritten);
            return entriesWritten == System.Net.HttpStatusCode.Created;
        }

        private async Task FetchPersistedVersionAsync(EnergySummary summary, CancellationToken cancellationToken)
        {
            if (_versionConfig.PersistedNumber != null) return;

            var previousIntervalStart = summary.IntervalStartIncluded - (summary.IntervalEndExcluded - summary.IntervalStartIncluded);
            var persistedRecord = await _EnergySummaryReadRepository.Get(summary.GetKey(), cancellationToken)
                ?? await _EnergySummaryReadRepository.Get(previousIntervalStart.GetIntervalKey(), cancellationToken);
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
