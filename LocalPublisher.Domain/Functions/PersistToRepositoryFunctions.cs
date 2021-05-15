using Domain;
using Entities;
using LazyCache;
using MediatR;
using Newtonsoft.Json;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository;
using Repository.Model;
using Serilog;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LocalPublisher.Domain.Functions
{
    public class IPersistToRepositoryFunctions
    {

    }
    public class PersistToRepositoryFunctions<TPersistedType, TReadModel> : IPersistToRepositoryFunctions 
        where TReadModel : class, IRepositoryReadModel
        where TPersistedType : IIntervalEntity
    {
        private readonly IDocumentReadRepository<TReadModel> _documentReadRepository;
        private readonly ISummaryRepository<TPersistedType> _documentWriteRepository;
        private readonly Func<TPersistedType, string> _getKey;
        private readonly Func<TPersistedType, string> _getKey2;
        private readonly IApplicationVersion _versionConfig;
        private readonly ILogger _logger;
        private readonly IRepoConfig _repoConfig;
        private readonly IAppCache _cache;
        private readonly string _cacheCollectionKey;
        private readonly IMediator _mediator;

        public PersistToRepositoryFunctions(
            IDocumentReadRepository<TReadModel> documentReadRepository,
            ISummaryRepository<TPersistedType> documentWriteRepository,
            IApplicationVersion versionConfig,
            ILogger logger,
            IRepoConfig repoConfig,
            IAppCache cache,
            string cacheCollectionKey,
            IMediator mediator,
            Func<TPersistedType, string> getKey,
            Func<TPersistedType, string> getKey2)
        {
            _documentReadRepository = documentReadRepository;
            _documentWriteRepository = documentWriteRepository;
            _getKey = getKey;
            _getKey2 = getKey2;
            _versionConfig = versionConfig;
            _logger = logger;
            _repoConfig = repoConfig;
            _cache = cache;
            _cacheCollectionKey = cacheCollectionKey;
            _mediator = mediator;
        }

        public async Task Handle(TPersistedType notification, CancellationToken cancellationToken)
        {
            if (_repoConfig.Testing) return;
            try
            {
                _logger.Verbose("Handling {Summary}", JsonConvert.SerializeObject(notification));
                var inMemoryCollection = GetCollection();
                var key = _getKey(notification);
                var added = inMemoryCollection.TryAdd(key, new PersistenceInfo<TPersistedType>(notification));
                if (!added)
                {
                    _logger.Error("Summary {Key} already added to collection", key);
                }
                await PersistAsync(key, cancellationToken);
                await PublishWaitingSummariesAsync(cancellationToken);
                _logger.Verbose("Handled {Summary}", _getKey(notification));
            }
            catch (Exception e)
            {
                _logger.Verbose(e, "This shouldn't be necessary, something higher should capture and log this message");
                throw;
            }
        }

        public async Task Handle(PersistenceInfo<TPersistedType> notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Verbose("Persistence error. Handling {Type} summary {Summary}", typeof(TPersistedType).Name, JsonConvert.SerializeObject(notification));
                var summary = notification.Summary;
                var verifyResult = await VerifyAsync(summary, cancellationToken);
                if (!verifyResult.isPersisted)
                {
                    notification.Increment();
                    var delay = TimeSpan.FromSeconds(Math.Min((int)Math.Pow(2, notification.GetRetryCount()), 60));
                    _logger.Error("Waiting {Seconds} before publishing {@Summary}", delay.TotalSeconds, summary);
                    await Task.Delay(delay, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                    await PersistAsync(_getKey(summary), cancellationToken);
                }
                else
                {
                    _logger.Warning("Rehandled {Type} summary has been persisted {Key} and removed {Removed}", typeof(TPersistedType).Name, _getKey(notification.Summary), verifyResult.isRemoved);
                }
                _logger.Verbose("Persistence error. Handled {Type} summary {Summary}", typeof(TPersistedType).Name, _getKey(notification.Summary));
            }
            catch (Exception e)
            {
                _logger.Verbose(e, "This shouldn't be necessary, something else should capture and log this message");
                throw;
            }
        }

        private ConcurrentDictionary<string, PersistenceInfo<TPersistedType>> GetCollection()
        {
            return _cache.GetOrAdd(_cacheCollectionKey, () => new ConcurrentDictionary<string, PersistenceInfo<TPersistedType>>(), DateTimeOffset.MaxValue);
        }

        private async Task<bool> IsPersisted(TPersistedType notification, CancellationToken cancellationToken)
        {
            var result = await _documentReadRepository.Get(_getKey(notification), cancellationToken);
            result = result ?? await _documentReadRepository.Get(_getKey2(notification), cancellationToken);
            // TODO implement extra check, to see if we want to also check the key has the same result as the true-key
            return result != null;
        }

        private async Task<bool> WriteAsync(TPersistedType notification, CancellationToken cancellationToken)
        {
            _logger.Information("Writing {Time}", _getKey(notification));
            var entriesWritten = await _documentWriteRepository.AddAsync(notification, cancellationToken);
            _logger.Information("Written {Time} {Entries}", _getKey(notification), entriesWritten);
            return entriesWritten;
        }

        private async Task FetchPersistedVersionAsync(TPersistedType summary, CancellationToken cancellationToken)
        {
            if (_versionConfig.PersistedNumber != null) return;

            var previousIntervalStart = summary.IntervalStartIncluded - (summary.IntervalEndExcluded - summary.IntervalStartIncluded);
            var persistedRecord = await _documentReadRepository.Get(_getKey(summary), cancellationToken)
                ?? await _documentReadRepository.Get(previousIntervalStart.GetIntervalKey(), cancellationToken);
            if (persistedRecord != null)
            {
                _versionConfig.PersistedNumber = persistedRecord.Version;
            }
        }

        private async Task<(bool isPersisted, bool isRemoved)> VerifyAsync(TPersistedType summary, CancellationToken cancellationToken)
        {
            bool isRemoved = false;
            await Task.Delay(TimeSpan.FromMilliseconds(_repoConfig.DelayMillisecondsBeforeVerify), cancellationToken);
            var isPersisted = await IsPersisted(summary, cancellationToken);
            if (isPersisted)
            {
                var inMemoryCollection = GetCollection();
                isRemoved = inMemoryCollection.TryRemove(_getKey(summary), out PersistenceInfo<TPersistedType> _);
            }
            return (isPersisted, isRemoved);
        }

        private async Task WriteAndVerifyAsync(TPersistedType notification, CancellationToken cancellationToken)
        {
            bool isPersisted = false, isRemoved = false;
            var written = await WriteAsync(notification, cancellationToken);
            if (written)
            {
                (isPersisted, isRemoved) = await VerifyAsync(notification, cancellationToken);
                if (written && isPersisted)
                {
                    _versionConfig.ThisProcessWrittenRecord = true;
                }
            }
            var logMethod = isPersisted ? (Action<string, object[]>)_logger.Information : _logger.Error;
            logMethod("Write and verify: {Type} {Key} written {Written} Verified {isPersisted} Removed {Removed}", new object[] { typeof(TPersistedType).Name, _getKey(notification), written, isPersisted, isRemoved });
        }

        private async Task PublishWaitingSummariesAsync(CancellationToken cancellationToken)
        {
            var inMemoryCollection = GetCollection();
            _logger.Verbose("Inmemory {Type} summary count {Count}", typeof(TPersistedType).Name, inMemoryCollection.Keys.Count);
            var inMemoryKey = inMemoryCollection.Keys.FirstOrDefault();
            PersistenceInfo<TPersistedType> summary;
            if (inMemoryKey != null)
                if (inMemoryCollection.TryGetValue(inMemoryKey, out summary))
                {
                    _logger.Information("Found an unprocessed count {Count} for item {Item}", summary.GetRetryCount(), JsonConvert.SerializeObject(summary));
                    if (!summary.IsProcessing)
                        await _mediator.Publish(summary, cancellationToken);
                }
        }

        private async Task PersistAsync(string key, CancellationToken cancellationToken)
        {
            var inMemoryCollection = GetCollection();
            if (!inMemoryCollection.TryGetValue(key, out PersistenceInfo<TPersistedType> item))
                return;
            try
            {
                item.IsProcessing = true;
                await FetchPersistedVersionAsync(item.Summary, cancellationToken);

                //if (_versionConfig.PersistedNumber == _versionConfig.Number && _versionConfig.ThisProcessWrittenRecord)
                    await WriteAndVerifyAsync(item.Summary, cancellationToken);
                //else
                //{
                //    await Task.Delay(TimeSpan.FromSeconds(_repoConfig.DelaySecondsBeforePersisting), cancellationToken);
                //    if (cancellationToken.IsCancellationRequested) return;
                //    var verifyResult = await VerifyAsync(item.Summary, cancellationToken);
                //    if (verifyResult.isPersisted)
                //    {
                //        await FetchPersistedVersionAsync(item.Summary, cancellationToken);
                //        if (_versionConfig.PersistedNumber == _versionConfig.Number)
                //        {
                //            _logger.Warning("Persisted number is equal, however another process has persisted this record {Type} {Key}. Item removed {Removed}", typeof(TPersistedType).Name, key, verifyResult.isRemoved);
                //        }
                //        else
                //        {
                //            _logger.Information("Previous version is running and has persisted this record {Type} {Key}. Item removed {Removed}", typeof(TPersistedType).Name, key, verifyResult.isRemoved);
                //        }
                //    }
                //    else
                //    {
                //        _logger.Information("This record {Type} {Key} has not been persisted. Attempting to write.", typeof(TPersistedType).Name, key, verifyResult.isRemoved);
                //        await WriteAndVerifyAsync(item.Summary, cancellationToken);
                //    }
                //}
            }
            catch (Microsoft.Azure.Cosmos.CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.Warning("Conflicted {Type} summary {Key}", typeof(TPersistedType).Name, key);
                await VerifyAsync(item.Summary, cancellationToken);
            }
            // timeout 408 received
            catch (Exception e)
            {
                _logger.Error(e, "{Type} summary persistence failure", typeof(TPersistedType).Name);
            }
            finally
            {
                item.IsProcessing = false;
            }
        }
    }
}
