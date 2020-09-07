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
using Serilog;

namespace Domain
{
    public class SenecEnergySummaryCommand : IRequest<Unit>
    {
    }

    public class SenecEnergySummaryCommandHandler : IRequestHandler<SenecEnergySummaryCommand, Unit>
    {
        private readonly IEnergyAdapter _gridMeterAdapter;
        private readonly ISenecCompressConfig _config;
        private readonly IMediator _mediator;
        private readonly IAppCache _cache;
        private readonly ILogger _logger;

        // TODO make a DI singleton
        private static object _lock = new object();

        public SenecEnergySummaryCommandHandler(
            IEnergyAdapter gridMeterAdapter,
            ISenecCompressConfig config,
            IMediator mediator,
            ILogger logger,
            IAppCache cache)
        {
            _gridMeterAdapter = gridMeterAdapter;
            _config = config;
            _mediator = mediator;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Unit> Handle(SenecEnergySummaryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<EnergySummary> tasks = GetSummaries();
                foreach (var voltageSummary in tasks)
                {
                    _logger.Verbose("Publishing {StartTime}", voltageSummary.IntervalStartIncluded);
                    await _mediator.Publish(voltageSummary, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
            }
            return Unit.Value;
        }

        private List<EnergySummary> GetSummaries()
        {
            _logger.Verbose("Getting summaries");
            lock (_lock)
            {
                _logger.Verbose("Getting summaries - inside lock");
                var tasks = new List<EnergySummary>();
                var collection = _cache.Get<ConcurrentDictionary<long, string>>(SmartMeterEnergyCache.CacheKey);
                while (collection != null && collection.Count > 0)
                {
                    _logger.Verbose("grid meter loop count {Count}", collection.Count);
                    var interval = GetMinimumInterval(collection);
                    var intervalPlusBuffer = interval.End.AddSeconds(10);
                    var internalLastAdded = GetLastTime(collection);
                    var isBufferBeyondLastItem = intervalPlusBuffer >= internalLastAdded;
                    _logger.Verbose("{intervalPlusBuffer} >= {internalLastAdded} is {Truthiness}", intervalPlusBuffer, internalLastAdded, isBufferBeyondLastItem);
                    if (isBufferBeyondLastItem) break;

                    _logger.Verbose("Creating {StartTime}", interval.Start);
                    var result = CreateVoltageSummary(collection, interval.Start, interval.End);
                    if (result == null) continue;
                    _logger.Verbose("Created {StartTime}", interval.Start);
                    tasks.Add(result);
                }
                _logger.Verbose("Getting summaries complete count {Count}", tasks.Count);
                return tasks;
            }
        }

        private (DateTimeOffset Start, DateTimeOffset End) GetMinimumInterval(ConcurrentDictionary<long, string> collection)
        {
            var firstItem = collection.Keys.Min();
            _logger.Verbose("Minimum in collection {Minimum} {Time}", firstItem, DateTimeOffset.FromUnixTimeSeconds(firstItem));
            var firstTime = DateTimeOffset.FromUnixTimeSeconds(firstItem);
            var frequency = TimeSpan.FromMinutes(_config.MinutesPerSummary);
            var minIntoInterval = firstTime.TimeOfDay.Ticks % frequency.Ticks;
            var intervalStart = firstTime.AddTicks(-minIntoInterval);
            var intervalEnd = intervalStart + frequency;
            return (intervalStart, intervalEnd);
        }

        private static DateTimeOffset GetLastTime(ConcurrentDictionary<long, string> collection)
        {
            var lastItem = collection.Keys.Max();
            return DateTimeOffset.FromUnixTimeSeconds(lastItem);
        }

        private EnergySummary? CreateVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
        {
            var removedTexts = new List<string>();
            try
            {
                return BuildVoltageSummary(collection, intervalStart, intervalEnd, removedTexts);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Create voltage summary error {@Values}", removedTexts);
                return null;
            }
        }

        private EnergySummary BuildVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, List<string> removedTexts)
        {
            var list = new List<MomentEnergy>();
            var lowerBound = intervalStart.ToUnixTimeSeconds();
            var upperBound = intervalEnd.ToUnixTimeSeconds();
            for (long instant = lowerBound; instant < upperBound; instant++)
            {
                if (!collection.TryRemove(instant, out string textValue)) continue;
                removedTexts.Add(textValue);
                var original = JsonConvert.DeserializeObject<SenecEntities.SmartMeterEnergy>(textValue);
                if (original == null) continue; // no way the programmer serialised nothing
                var entity = _gridMeterAdapter.Convert(instant, original.ENERGY);
                var voltages = entity.GetMomentEnergy();
                if (voltages.IsValid)
                    list.Add(voltages);
            }

            var maximumValues = (int)(intervalEnd - intervalStart).TotalSeconds;
            var stats = new EnergySummary(
                intervalStartIncluded: intervalStart,
                intervalEndExcluded: intervalEnd,
                batteryPercentageFull: new decimal?().Value,
                gridExportWattHours: new decimal ?().Value,
                gridImportWattHours: new decimal ?().Value,
                consumptionWattHours: new decimal ?().Value,
                solarPowerGenerationWattHours: new decimal ?().Value,
                batteryChargeWattHours: new decimal ?().Value,
                batteryDischargeWattHours: new decimal ?().Value,
                new int?().Value,
                new int?().Value,
                new int?().Value
                );
            return stats;
        }

        private Statistic CreateStatistics(List<MomentVoltage> list, Func<MomentVoltage, decimal> property, int maximumValues)
        {
            var values = (
                from a in list
                let value = property(a)
                orderby value
                select value
                ).ToList();
            var failures = maximumValues - values.Count;
            if (values.Count > 0)
            {
                var minimum = values.First();
                var maximum = values.Last();
                var midPoint = values.Count % 2 == 0
                    ? values.Count / 2 - 1
                    : values.Count / 2;
                var median = values[midPoint];
                return new Statistic(minimum, maximum, median, failures);
            }
            return new Statistic(failures);
        }
    }
}
