using Entities;
using LazyCache;
using MediatR;
using Newtonsoft.Json;
using SenecEntities;
using SenecEntitiesAdapter;
using Serilog;
using System.Collections.Concurrent;
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
            Writefile(notification, time);
            _logger.Information("Logged {Time}", time.Value);
            return Task.CompletedTask;
        }

        private void Writefile(GridMeter notification, SenecDecimal time)
        {
            var collection = _cache.GetOrAdd("gridmeter", () => new ConcurrentDictionary<long, string>());
            collection.TryAdd((long)time.Value.Value, JsonConvert.SerializeObject(notification.PM1OBJ1));
        }
    }
}
