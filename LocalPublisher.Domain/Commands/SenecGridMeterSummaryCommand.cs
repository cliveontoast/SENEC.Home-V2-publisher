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
using Shared;

namespace Domain
{
    public class SenecGridMeterSummaryCommand : IRequest<Unit>
    {
    }

    public class SenecGridMeterSummaryCommandHandler : IRequestHandler<SenecGridMeterSummaryCommand, Unit>
    {
        private readonly IGridMeterAdapter _gridMeterAdapter;
        private readonly ISenecVoltCompressConfig _config;
        private readonly IMediator _mediator;
        private readonly IAppCache _cache;
        private readonly ILogger _logger;
        private readonly IZoneProvider _zoneProvider;

        // TODO make a DI singleton
        private static object _lock = new object();

        public SenecGridMeterSummaryCommandHandler(
            IGridMeterAdapter gridMeterAdapter,
            ISenecVoltCompressConfig config,
            IMediator mediator,
            ILogger logger,
            IZoneProvider zoneProvider,
            IAppCache cache)
        {
            _gridMeterAdapter = gridMeterAdapter;
            _config = config;
            _mediator = mediator;
            _logger = logger;
            _zoneProvider = zoneProvider;
            _cache = cache;

            _cache.GetOrAdd(GridMeterCache.CacheKey, () => new ConcurrentDictionary<long, string>());
        }

        public async Task<Unit> Handle(SenecGridMeterSummaryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<VoltageSummary> tasks = GetSummaries();
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

        private List<VoltageSummary> GetSummaries()
        {
            _logger.Verbose("Getting summaries");
            lock (_lock)
            {
                _logger.Verbose("Getting summaries - inside lock");
                var tasks = new List<VoltageSummary>();
                var collection = _cache.Get<ConcurrentDictionary<long, string>>(GridMeterCache.CacheKey);
                while (collection != null && collection.Count > 0)
                {
                    _logger.Verbose("grid meter loop count {Count}", collection.Count);
                    var interval = GetMinimumInterval(collection);
                    var intervalPlusBuffer = interval.End.AddSeconds(10);
                    var internalLastAdded = SenecEnergySummaryCommandHandler.GetLastTime(collection, _zoneProvider);
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
            return SenecEnergySummaryCommandHandler.GetMinimumInterval(collection, _zoneProvider, _logger, _config.MinutesPerSummary);
        }

        private VoltageSummary? CreateVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
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

        private VoltageSummary BuildVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, List<string> removedTexts)
        {
            var list = new List<MomentVoltage>();
            var lowerBound = intervalStart.ToUnixTimeSeconds();
            var upperBound = intervalEnd.ToUnixTimeSeconds();
            for (long instant = lowerBound; instant < upperBound; instant++)
            {
                if (!collection.TryRemove(instant, out string textValue)) continue;
                removedTexts.Add(textValue);
                var original = JsonConvert.DeserializeObject<SenecEntities.Meter>(textValue);
                if (original == null) continue; // no way the programmer serialised nothing
                var moment = instant.ToEquipmentLocalTime(_zoneProvider);
                var entity = _gridMeterAdapter.Convert(moment, original);
                var voltages = entity.GetVoltageMoment();
                if (voltages.IsValid)
                    list.Add(voltages);
            }

            var maximumValues = (int)(intervalEnd - intervalStart).TotalSeconds;
            var stats = new VoltageSummary(
                intervalStartIncluded: intervalStart,
                intervalEndExcluded: intervalEnd,
                l1: CreateStatistics(list, a => a.L1, maximumValues),
                l2: CreateStatistics(list, a => a.L2, maximumValues),
                l3: CreateStatistics(list, a => a.L3, maximumValues)
                );
            return stats;
        }

        private StatisticV1 CreateStatistics(List<MomentVoltage> list, Func<MomentVoltage, decimal> property, int maximumValues)
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
                return new StatisticV1(minimum, maximum, median, failures);
            }
            return new StatisticV1(failures);
        }
    }
}
