using Entities;
using LazyCache;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository;
using Repository.Model;
using Serilog;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalPublisher.Domain.Functions
{
    public class IPersistToRepositoryFunctions
    {

    }
    public class PersistToRepositoryFunctions<TPersistedType, TReadModel> : IPersistToRepositoryFunctions 
        where TReadModel : class, IRepositoryReadModel
        where TPersistedType : /*IRepositoryEntity,*/ IIntervalEntity
    {
        private readonly IDocumentReadRepository<TReadModel> _documentReadRepository;
        private readonly ISummaryRepository<TPersistedType> _documentWriteRepository;
        private readonly Func<TPersistedType, string> _getKey;
        private readonly Func<TPersistedType, string> _getKey2;
        private readonly IApplicationVersion _versionConfig;
        private readonly ILogger _logger;
        private readonly IAppCache _cache;
        private readonly string _cacheCollectionKey;

        public PersistToRepositoryFunctions(
            IDocumentReadRepository<TReadModel> documentReadRepository,
            ISummaryRepository<TPersistedType> documentWriteRepository,
            IApplicationVersion versionConfig,
            ILogger logger,
            LazyCache.IAppCache cache,
            string cacheCollectionKey,
            Func<TPersistedType, string> getKey,
            Func<TPersistedType, string> getKey2
            )
        {
            _documentReadRepository = documentReadRepository;
            _documentWriteRepository = documentWriteRepository;
            _getKey = getKey;
            _getKey2 = getKey2;
            _versionConfig = versionConfig;
            _logger = logger;
            _cache = cache;
            _cacheCollectionKey = cacheCollectionKey;
        }

        public ConcurrentDictionary<string, PersistenceInfo<TPersistedType>> GetCollection()
        {
            return _cache.GetOrAdd(_cacheCollectionKey, () => new ConcurrentDictionary<string, PersistenceInfo<TPersistedType>>(), DateTimeOffset.MaxValue);
        }

        public async Task<bool> IsPersisted(TPersistedType notification, CancellationToken cancellationToken)
        {
            var result = await _documentReadRepository.Get(_getKey(notification), cancellationToken);
            result = result ?? await _documentReadRepository.Get(_getKey2(notification), cancellationToken);
            return result != null;
        }

        public async Task<bool> WriteAsync(TPersistedType notification, CancellationToken cancellationToken)
        {
            _logger.Information("Writing {Time}", _getKey(notification));
            var entriesWritten = await _documentWriteRepository.AddAsync(notification, cancellationToken);
            _logger.Information("Written {Time} {Entries}", _getKey(notification), entriesWritten);
            return entriesWritten;
        }

        public async Task FetchPersistedVersionAsync(TPersistedType summary, CancellationToken cancellationToken)
        {
            if (_versionConfig.PersistedNumber != null) return;

            var previousIntervalStart = summary.IntervalStartIncluded - (summary.IntervalEndExcluded - summary.IntervalStartIncluded);
            var persistedRecord = await _documentReadRepository.Get(_getKey(summary), cancellationToken)
                ?? await _documentReadRepository.Get(previousIntervalStart.GetIntervalKey(), cancellationToken);
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
