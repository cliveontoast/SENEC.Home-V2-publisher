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
            var time = _adapter.GetDecimal(notification.RTC.WEB_TIME);
            if (time?.Value.HasValue == true)
                Writefile(notification, time, cacheKey);
            else
                _logger.Error("{TypeName} has no time value - {@Notification}", notification.GetType().Name, notification);
        }

        private void Writefile(IRealTimeNotification notification, SenecDecimal time, string cacheKey)
        {
            var collection = _cache.GetOrAdd(cacheKey, () => new ConcurrentDictionary<long, string>(), DateTimeOffset.MaxValue);
            if (collection.TryAdd((long)time.Value.Value, JsonConvert.SerializeObject(notification.SerializableEntity)))
                _logger.Verbose("Logged {UnixTime} {Time} Cache count {Count}", time.Value, DateTimeOffset.FromUnixTimeSeconds((long)time.Value.Value), collection.Count);
            else
                _logger.Warning("Logged {UnixTime} could not add {TypeName} value to memory collection. Count {Count}", time.Value, notification.GetType().Name, collection.Count);
        }
    }
}
