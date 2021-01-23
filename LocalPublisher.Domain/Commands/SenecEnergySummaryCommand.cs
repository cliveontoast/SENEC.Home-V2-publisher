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
using LocalPublisher.Domain.Functions;

namespace Domain
{
    public class SenecEnergySummaryCommand : IRequest<Unit>
    {
    }

    public class SenecEnergySummaryCommandHandler : IRequestHandler<SenecEnergySummaryCommand, Unit>
    {
        private readonly ISummaryFunctions _summaryFunctions;
        private readonly IEnergyAdapter _gridMeterAdapter;
        private readonly IAppCache _cache;

        public SenecEnergySummaryCommandHandler(
            ISummaryFunctions summaryFunctions,
            IEnergyAdapter gridMeterAdapter,
            ISenecEnergyCompressConfig config,
            ILogger logger,
            IAppCache cache)
        {
            _summaryFunctions = summaryFunctions;
            _gridMeterAdapter = gridMeterAdapter;
            _cache = cache;
            _cache.GetOrAdd(SolarPowerCache.CacheKey, () => new ConcurrentDictionary<long, string>());
            _summaryFunctions.Initialise(logger, config, SmartMeterEnergyCache.CacheKey, BuildVoltageSummary, "energy");
        }

        public async Task<Unit> Handle(SenecEnergySummaryCommand request, CancellationToken cancellationToken)
        {
            return await _summaryFunctions.Handle(cancellationToken);
        }

        private Dictionary<long, decimal> GetSolarValues(long lowerBound, long upperBoundExclusive)
        {
            var newCache = new Dictionary<long, decimal>();
            var cache = _cache.Get<ConcurrentDictionary<long, string>>(SolarPowerCache.CacheKey);
            for (var i = lowerBound; i < upperBoundExclusive; i++)
            {
                if (!cache.TryRemove(i, out var value)) continue;
                if (!decimal.TryParse(value, out decimal decimalValue)) continue;

                newCache.Add(i, decimalValue);
            }
            return newCache;
        }

        private IEnumerable<IIntervalEntity> BuildVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, List<string> removedTexts, CancellationToken cancellationToken)
        {
            Dictionary<long, decimal> solarValues = null!;
            var list = _summaryFunctions.FillSummary<MomentEnergy, SenecEntities.Energy>(
                cancellationToken,
                collection,
                intervalStart,
                intervalEnd,
                converter: (moment, energy) => _gridMeterAdapter.Convert(moment, energy).GetMomentEnergy(),
                removedTexts,
                lowerUpperBoundExtras: (lowerBound, upperBound) => solarValues = GetSolarValues(lowerBound, upperBound),
                postConvertAction: (momentEnergy, instant) =>
                {
                    if (solarValues.TryGetValue(instant, out decimal solarValue))
                        momentEnergy.SolarInvertorsPowerGeneration = solarValue;
                    momentEnergy.PowerMovements = PowerMovementsBuilder.Build(momentEnergy);
                });

            var maximumValues = (int)(intervalEnd - intervalStart).TotalSeconds;
            var stats = new EnergySummary(
                intervalStartIncluded: intervalStart,
                intervalEndExcluded: intervalEnd,
                batteryPercentageFull: CreateStatistics(list, l => l.BatteryPercentageFull),
                gridExportWatts: CreateStatistics(list, l => l.GridExportWatts),
                gridExportWattEnergy: TimeseriesSummary(list, l => l.GridExportWatts, intervalStart, intervalEnd),
                gridImportWatts: CreateStatistics(list, l => l.GridImportWatts),
                gridImportWattEnergy: TimeseriesSummary(list, l => l.GridImportWatts, intervalStart, intervalEnd),
                estimateConsumptionWatts: CreateStatistics(list, l => l.HomeInstantPowerConsumption),
                estimateConsumptionWattEnergy: TimeseriesSummary(list, l => l.HomeInstantPowerConsumption, intervalStart, intervalEnd),
                batteryReportedSolarPowerGenerationWatts: CreateStatistics(list, l => l.SolarPowerGeneration),
                batteryReportedSolarPowerGenerationWattEnergy: TimeseriesSummary(list, l => l.SolarPowerGeneration, intervalStart, intervalEnd),
                solarInverterPowerGenerationWatts: CreateStatistics(solarValues.Values.ToList()),
                solarInverterPowerGenerationWattEnergy: TimeseriesSummary(solarValues, intervalStart, intervalEnd),
                batteryChargeWatts: CreateStatistics(list, l => l.BatteryCharge),
                batteryChargeWattEnergy: TimeseriesSummary(list, l => l.BatteryCharge, intervalStart, intervalEnd),
                batteryDischargeWatts: CreateStatistics(list, l => l.BatteryDischarge),
                batteryDischargeWattEnergy: TimeseriesSummary(list, l => l.BatteryDischarge, intervalStart, intervalEnd),
                powerMovementSummary: CreatePowerSummary(list, intervalStart, intervalEnd),
                secondsBatteryCharging: list.Count(a => a.IsBatteryCharging),
                secondsBatteryDischarging: list.Count(a => a.IsBatteryDischarging),
                secondsWithoutData: maximumValues - list.Count
                );
            var states = new EquipmentStatesSummary(
                intervalStartIncluded: stats.IntervalStartIncluded,
                intervalEndExcluded: stats.IntervalEndExcluded,
                states: CreateTextStatistics(list, l => l.SystemState),
                secondsWithoutData: stats.SecondsWithoutData
                );
            return new IIntervalEntity[] { stats, states };
        }

        private PowerMovementSummary CreatePowerSummary(List<MomentEnergy> list, DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
        {
            return new PowerMovementSummary(
                batteryToGridWatts: CreateStatistics(list, l => l.PowerMovements?.BatteryToGrid),
                batteryToHomeWatts: CreateStatistics(list, l => l.PowerMovements?.BatteryToHome),
                gridToBatteryWatts: CreateStatistics(list, l => l.PowerMovements?.GridToBattery),
                gridToHomeWatts: CreateStatistics(list, l => l.PowerMovements?.GridToHome),
                solarToBatteryWatts: CreateStatistics(list, l => l.PowerMovements?.SolarToBattery),
                solarToGridWatts: CreateStatistics(list, l => l.PowerMovements?.SolarToGrid),
                solarToHomeWatts: CreateStatistics(list, l => l.PowerMovements?.SolarToHome),
                batteryToGridWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.BatteryToGrid, intervalStart, intervalEnd),
                batteryToHomeWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.BatteryToHome, intervalStart, intervalEnd),
                gridToBatteryWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.GridToBattery, intervalStart, intervalEnd),
                gridToHomeWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.GridToHome, intervalStart, intervalEnd),
                solarToBatteryWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.SolarToBattery, intervalStart, intervalEnd),
                solarToGridWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.SolarToGrid, intervalStart, intervalEnd),
                solarToHomeWattEnergy: TimeseriesSummary(list, l => l.PowerMovements?.SolarToHome, intervalStart, intervalEnd)
                );
        }

        private decimal TimeseriesSummary(List<MomentEnergy> list, Func<MomentEnergy, decimal?> property, DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
        {
            var query = (
                from i in list
                let value = property(i)
                where value.HasValue
                select new { instant = i.Instant.ToUnixTimeSeconds(), value.Value })
                .ToDictionary(i => i.instant, i => i.Value);
            return TimeseriesSummary(query, intervalStart, intervalEnd);
        }

        private decimal TimeseriesSummary(Dictionary<long, decimal> index, DateTimeOffset intervalStartTime, DateTimeOffset intervalEndTime)
        {
            long intervalStart = intervalStartTime.ToUnixTimeSeconds();
            long intervalEnd = intervalEndTime.ToUnixTimeSeconds();
            decimal lastValue = 0;
            decimal sum = 0;
            long instant = intervalStart;
            while (instant <= intervalEnd)
            {
                if (index.ContainsKey(instant))
                {
                    var value = index[instant];
                    lastValue = value;
                }
                sum += lastValue;
                instant++;
            }

            return sum;
        }

        private IEnumerable<EquipmentStateStatistic> CreateTextStatistics(List<MomentEnergy> list, Func<MomentEnergy, string> property)
        {
            var values =
                from a in list
                let value = property(a)
                group value by value into textGroup
                select new EquipmentStateStatistic(textGroup.Key, textGroup.Count());
            return values.ToArray();
        }

        private Statistic CreateStatistics(List<MomentEnergy> list, Func<MomentEnergy, decimal?> property)
        {
            var values = (
                from a in list
                let value = property(a)
                where value.HasValue
                orderby value.Value
                select value.Value
                ).ToList();
            return CreateStatistics(values);
        }

        private static Statistic CreateStatistics(List<decimal> values)
        {
            if (values.Count > 0)
            {
                var minimum = values.First();
                var maximum = values.Last();
                var midPoint = values.Count % 2 == 0
                    ? values.Count / 2 - 1
                    : values.Count / 2;
                var median = values[midPoint];
                return new Statistic(minimum, maximum, median, decimal.Round(values.Average(), 2));
            }
            return new Statistic(false);
        }
    }
}
