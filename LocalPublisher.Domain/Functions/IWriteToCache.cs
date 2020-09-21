using Entities;
using LazyCache;
using Newtonsoft.Json;
using SenecEntities;
using SenecEntitiesAdapter;
using Serilog;
using System;
using System.Collections.Concurrent;

namespace LocalPublisher.Domain.Functions
{
    public interface IWriteToCache
    {
        void Add(IRealTimeNotification notification, string cacheKey);
    }

    public class WriteToCache : IWriteToCache
    {
        private readonly ILogger _logger;
        private readonly IAppCache _cache;
        private readonly IAdapter _adapter;

        public WriteToCache(
            ILogger logger,
            IAppCache cache,
            IAdapter adapter)
        {
            _logger = logger;
            _cache = cache;
            _adapter = adapter;
        }

        public void Add(IRealTimeNotification notification, string cacheKey)
        {
            Writefile(notification, cacheKey);
        }

        private void Writefile(IRealTimeNotification notification, string cacheKey)
        {
            var unixMoment = notification.ReceivedUnixMillisecondsTimestamp / 1000;
            var moment = DateTimeOffset.FromUnixTimeMilliseconds(unixMoment);
            var collection = _cache.GetOrAdd(cacheKey, () => new ConcurrentDictionary<long, string>(), DateTimeOffset.MaxValue);
            if (collection.TryAdd(unixMoment, JsonConvert.SerializeObject(notification.SerializableEntity)))
                _logger.Verbose("Cached {UnixTime} {Time} - Cache count {Count}", unixMoment, moment, collection.Count);
            else
                _logger.Information("Cached {UnixTime} already exists for {TypeName} - Cache count {Count}", unixMoment, notification.GetType().Name, collection.Count);
        }
    }
}
