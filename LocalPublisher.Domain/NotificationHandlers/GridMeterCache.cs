using Entities;
using LazyCache;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SenecEntities;
using SenecEntitiesAdapter;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class GridMeterCache : INotificationHandler<GridMeter>
    {
        private readonly ILogger _logger;
        private readonly IAppCache _cache;
        private readonly IAdapter _adapter;

        public GridMeterCache(
            ILogger logger,
            IAppCache cache,
            IAdapter adapter)
        {
            _logger = logger;
            _cache = cache;
            _adapter = adapter;
        }

        public Task Handle(GridMeter notification, CancellationToken cancellationToken)
        {
            var time = _adapter.GetDecimal(notification.RTC.WEB_TIME);
            if (time?.Value.HasValue == true)
                Writefile(notification, time);
            else
                _logger.Error("Grid meter has no time value - {@GridMeter}", notification);
            return Task.CompletedTask;
        }

        private void Writefile(GridMeter notification, SenecDecimal time)
        {
            var collection = _cache.GetOrAdd("gridmeter", () => new ConcurrentDictionary<long, string>(), DateTimeOffset.MaxValue);
            if (collection.TryAdd((long)time.Value.Value, JsonConvert.SerializeObject(notification.PM1OBJ1)))
                _logger.Verbose("Logged {UnixTime} {Time} Cache count {Count}", time.Value, DateTimeOffset.FromUnixTimeSeconds((long)time.Value.Value), collection.Count);
            else
                _logger.Warning("Logged {UnixTime} could not add grid meter value to memory collection. Count {Count}", time.Value, collection.Count);
        }
    }
}
