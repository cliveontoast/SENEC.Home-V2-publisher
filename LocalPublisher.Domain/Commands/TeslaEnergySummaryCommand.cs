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
using TeslaEntities;

namespace Domain
{
    public class TeslaEnergySummaryCommand : IRequest<Unit>
    {
    }

    public class TeslaEnergySummaryCommandHandler : IRequestHandler<TeslaEnergySummaryCommand, Unit>
    {
        private readonly ISummaryFunctions _summaryFunctions;
        private readonly ILogger _logger;
        private readonly IAppCache _cache;

        public TeslaEnergySummaryCommandHandler(
            ISummaryFunctions summaryFunctions,
            ITeslaEnergyCompressConfig config,
            ILogger logger,
            IAppCache cache)
        {
            _summaryFunctions = summaryFunctions;
            _logger = logger;
            _cache = cache;
            _cache.GetOrAdd(TeslaStateOfChargeCache.CacheKey, () => new ConcurrentDictionary<long, string>());
            _summaryFunctions.Initialise(logger, config, TeslaSmartMeterEnergyCache.CacheKey, BuildVoltageSummary, "energy");
        }

        public async Task<Unit> Handle(TeslaEnergySummaryCommand request, CancellationToken cancellationToken)
        {
            return await _summaryFunctions.Handle(cancellationToken);
        }

        private Dictionary<long, decimal> GetStateOfChargeValues(long lowerBound, long upperBoundExclusive)
        {
            var newCache = new Dictionary<long, decimal>();
            var cache = _cache.Get<ConcurrentDictionary<long, string>>(TeslaStateOfChargeCache.CacheKey);
            for (var i = lowerBound; i < upperBoundExclusive; i++)
            {
                if (!cache.TryRemove(i, out var value)) continue;
                var obj = JsonConvert.DeserializeObject<StateOfEnergy>(value);
                if (obj.percentage != null)
                    newCache.Add(i, obj.percentage.Value);
            }
            return newCache;
        }

        private IEnumerable<IIntervalEntity> BuildVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, List<string> removedTexts, CancellationToken cancellationToken)
        {
            Dictionary<long, decimal> stateOfChargeValues = null!;
            var list = _summaryFunctions.FillSummary<MomentEnergy, MetersAggregates>(
                cancellationToken,
                collection,
                intervalStart,
                intervalEnd,
                converter: (moment, energy) => GetMomentEnergy(moment, energy, stateOfChargeValues),
                removedTexts,
                lowerUpperBoundExtras: (lowerBound, upperBound) => stateOfChargeValues = GetStateOfChargeValues(lowerBound, upperBound),
                postConvertAction: (momentEnergy, instant) =>
                {
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
                solarInverterPowerGenerationWatts: CreateStatistics(stateOfChargeValues.Values.ToList()),
                solarInverterPowerGenerationWattEnergy: TimeseriesSummary(stateOfChargeValues, intervalStart, intervalEnd),
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
                states: CreateTextStatistics(list, l => l.PowerMovements?.PowerState.ToString() ?? "Unknown"),
                secondsWithoutData: stats.SecondsWithoutData
                );
            return new IIntervalEntity[] { stats, states };
        }

        private MomentEnergy GetMomentEnergy(DateTimeOffset moment, MetersAggregates energy, Dictionary<long, decimal> stateOfChargeValues)
        {
            var stateOfCharge = (decimal?)null;
            if (stateOfChargeValues.TryGetValue(moment.ToUnixTimeSeconds(), out decimal stateOfChargeValue))
                stateOfCharge = stateOfChargeValue;

            var gridImportWatts = energy.site?.instant_power > 0 ? energy.site.instant_power : 0;
            var gridExportWatts = energy.site?.instant_power < 0 ? -energy.site.instant_power : 0;
            var batteryChargeWatts = energy.battery?.instant_power < 0 ? -energy.battery.instant_power : 0;
            var batteryDischargeWatts = energy.battery?.instant_power > 0 ? energy.battery.instant_power : 0;
            var homeConsumedWatts = energy.load?.instant_power > 0 ? energy.load.instant_power : 0;
            var solarGeneratedWatts = energy.solar?.instant_power > 15 ? energy.solar?.instant_power : 0; // negative is possible.  this should never be higher than 100 Watts. On occasion I see +/- -10 at night.
            homeConsumedWatts = CalculateConsumption(solarGeneratedWatts, energy.site?.instant_power, energy.battery?.instant_power, homeConsumedWatts);
            var result = new MomentEnergy(
                moment,
                stateOfCharge ?? -1,
                "",
                gridExportWatts,
                gridImportWatts,
                batteryDischargeWatts,
                batteryChargeWatts,
                batteryChargeWatts > 0,
                batteryDischargeWatts > 0,
                homeConsumedWatts,
                solarGeneratedWatts);
            return result;
        }

        private decimal CalculateConsumption(decimal? solar, decimal? grid, decimal? battery, decimal? home)
        {
            var calculated = grid.GetValueOrDefault() + battery.GetValueOrDefault() + solar.GetValueOrDefault();
            if (Math.Abs(calculated - home.GetValueOrDefault()) > 30)
                _logger.Warning("Reported home is way out man solar: {Solar} {Grid} {Battery} {Home} {Calculated}", solar, grid, battery, home, calculated);
            return calculated;
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
