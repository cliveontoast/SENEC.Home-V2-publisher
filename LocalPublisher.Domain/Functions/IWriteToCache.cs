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
            if (time.IsInteger)
                Writefile(notification, time.AsInteger, cacheKey);
            else
                _logger.Error("{TypeName} has no time value - {@Notification}", notification.GetType().Name, notification);
        }

        private void Writefile(IRealTimeNotification notification, long unixMoment, string cacheKey)
        {
            var moment = DateTimeOffset.FromUnixTimeSeconds(unixMoment);
            var collection = _cache.GetOrAdd(cacheKey, () => new ConcurrentDictionary<long, string>(), DateTimeOffset.MaxValue);
            if (collection.TryAdd(unixMoment, JsonConvert.SerializeObject(notification.SerializableEntity)))
                _logger.Verbose("Logged {UnixTime} {Time} Cache count {Count}", unixMoment, moment, collection.Count);
            else
                _logger.Information("Logged {UnixTime} could not add {TypeName} value to memory collection. Count {Count}", unixMoment, notification.GetType().Name, collection.Count);
        }
    }
}
