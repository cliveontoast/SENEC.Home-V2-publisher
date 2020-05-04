using LazyCache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using SenecEntitesAdapter;
using Entities;
using MediatR;
using System.Threading.Tasks;

namespace Domain
{
    public class SenecGridMeterSummaryCommand : IRequest<Unit>
    {
    }

    public class SenecGridMeterSummaryCommandHandler : IRequestHandler<SenecGridMeterSummaryCommand, Unit>
    {
        private readonly IGridMeterAdapter _gridMeterAdapter;
        private readonly ISenecCompressConfig _config;
        private readonly IMediator _mediator;
        private readonly IAppCache _cache;
        private static object _lock = new object();

        public SenecGridMeterSummaryCommandHandler(
            IGridMeterAdapter gridMeterAdapter,
            ISenecCompressConfig config,
            IMediator mediator,
            IAppCache cache)
        {
            _gridMeterAdapter = gridMeterAdapter;
            _config = config;
            _mediator = mediator;
            _cache = cache;
        }

        public Task<Unit> Handle(SenecGridMeterSummaryCommand request, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                var collection = _cache.Get<ConcurrentDictionary<long, string>>("gridmeter");
                while (collection != null && collection.Count > 0)
                {
                    var firstItem = collection.Keys.Min();
                    var firstTime = DateTimeOffset.FromUnixTimeSeconds(firstItem);
                    var lastItem = collection.Keys.Max();
                    var lastTime = DateTimeOffset.FromUnixTimeSeconds(lastItem);

                    var frequency = TimeSpan.FromMinutes(_config.MinutesPerSummary);
                    var minIntoInterval = firstTime.TimeOfDay.Ticks % frequency.Ticks;
                    var intervalStart = firstTime.AddTicks(-minIntoInterval);
                    var intervalEnd = intervalStart + frequency;
                    if (intervalEnd.AddSeconds(10) >= lastTime) break;

                    var result = CreateVoltageSummary(collection, intervalStart, intervalEnd);

                    _mediator.Publish(result, cancellationToken);
                }
                return Unit.Task;
            }
        }

        private VoltageSummary CreateVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
        {
            List<Meter> list = new List<Meter>();
            var lowerBound = intervalStart.ToUnixTimeSeconds();
            var upperBound = intervalEnd.ToUnixTimeSeconds();
            for (long instant = lowerBound; instant < upperBound; instant++)
            {
                if (!collection.TryRemove(instant, out string textValue)) continue;
                var original = JsonConvert.DeserializeObject<SenecEntities.Meter>(textValue);
                var entity = _gridMeterAdapter.Convert(instant, original);
                if (entity != null)
                    list.Add(entity);
            }

            var maximumValues = (int)(intervalEnd - intervalStart).TotalSeconds;
            var stats = new VoltageSummary()
            {
                IntervalStartIncluded = intervalStart,
                IntervalEndExcluded = intervalEnd,
                L1 = CreateStatistics(list, a => a.L1.Voltage, maximumValues),
                L2 = CreateStatistics(list, a => a.L2.Voltage, maximumValues),
                L3 = CreateStatistics(list, a => a.L3.Voltage, maximumValues),
            };
            return stats;
        }

        private Statistic CreateStatistics(List<Meter> list, Func<Meter, SenecDecimal> property, int maximumValues)
        {
            var result = new Statistic();
            var values = (
                from a in list
                let p = property(a)
                where p?.Value.HasValue == true
                let value = p.Value.Value
                orderby value
                select value
                ).ToList();
            result.Failures = maximumValues - values.Count;
            if (values.Count > 0)
            {
                result.Minimum = values.First();
                result.Maximum = values.Last();
                var midPoint = values.Count % 2 == 0
                    ? values.Count / 2 - 1
                    : values.Count / 2;
                result.Median = values[midPoint];
            }

            return result;
        }
    }
}
