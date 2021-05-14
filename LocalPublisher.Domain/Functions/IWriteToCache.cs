using LazyCache;
using Newtonsoft.Json;
using SenecEntities;
using Serilog;
using Shared;
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
        private readonly IZoneProvider _zoneProvider;

        public WriteToCache(
            ILogger logger,
            IZoneProvider zoneProvider,
            IAppCache cache)
        {
            _logger = logger;
            _cache = cache;
            _zoneProvider = zoneProvider;
        }

        public void Add(IRealTimeNotification notification, string cacheKey)
        {
            Writefile(notification, cacheKey);
        }

        private void Writefile(IRealTimeNotification notification, string cacheKey)
        {
            var unixMoment = notification.ReceivedUnixMillisecondsTimestamp / 1000;
            var collection = _cache.GetOrAdd(cacheKey, () => new ConcurrentDictionary<long, string>(), DateTimeOffset.MaxValue);
            if (collection.TryAdd(unixMoment, JsonConvert.SerializeObject(notification.SerializableEntity))) {
                var moment = DateTimeOffset.FromUnixTimeSeconds(unixMoment).ToEquipmentLocalTime(_zoneProvider);
                _logger.Verbose("Cached {UnixTime} {Time} - Cache count {Count}", unixMoment, moment, collection.Count);
            }
            else
                _logger.Debug("Cached {UnixTime} already exists for {TypeName} - Cache count {Count}", unixMoment, notification.GetType().Name, collection.Count);
        }
    }
}
