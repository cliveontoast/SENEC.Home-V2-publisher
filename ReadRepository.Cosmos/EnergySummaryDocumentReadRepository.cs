using Microsoft.Azure.Cosmos.Linq;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Repository.Model;

namespace ReadRepository.Cosmos
{
    public class EnergySummaryDocumentReadRepository : IEnergySummaryDocumentReadRepository
    {
        private readonly IReadContext _readContext;

        public EnergySummaryDocumentReadRepository(
            IReadContext readContext)
        {
            _readContext = readContext;
        }

        public async Task<EnergySummaryReadModel?> Get(string key, CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<EnergySummaryEntity>();
            var iterator = queryable.Where(p => p.Partition == "ES_" + key).ToFeedIterator();
            var result = await iterator.ReadNextAsync();
            var response = ToReadModel(result);
            return response.FirstOrDefault();
        }

        public async Task<PowerMovementSummaryReadModel> GetPowerMovement(string key, CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<EnergySummaryEntity>();
            var iterator = queryable.Where(p => p.Partition == "ES_" + key).ToFeedIterator();
            var result = await iterator.ReadNextAsync();
            var response = ToMovementReadModel(result);
            return response.FirstOrDefault();
        }
        public async Task<IEnumerable<PowerMovementSummaryReadModel>> FetchPowerMovements(DateTime date)
        {
            var results = await FetchRaw(date);
            return ToMovementReadModel(results);
        }

        private IEnumerable<PowerMovementSummaryReadModel> ToMovementReadModel(IEnumerable<EnergySummaryEntity> iterator)
        {
            var response = (
                from a in iterator
                where a.PowerMovementSummary != null
                let p = a.PowerMovementSummary
                select new PowerMovementSummaryReadModel(
                    intervalEndExcluded: a.IntervalEndExcluded,
                    intervalStartIncluded: a.IntervalStartIncluded,
                    key: a.Id,
                    version: a.Version,
                    solarToGridWatts: p.SolarToGridWatts,
                    solarToGridWattEnergy: p.SolarToGridWattEnergy,
                    solarToBatteryWatts: p.SolarToBatteryWatts,
                    solarToBatteryWattEnergy: p.SolarToBatteryWattEnergy,
                    solarToHomeWatts: p.SolarToHomeWatts,
                    solarToHomeWattEnergy: p.SolarToHomeWattEnergy,
                    batteryToHomeWatts: p.BatteryToHomeWatts,
                    batteryToHomeWattEnergy: p.BatteryToHomeWattEnergy,
                    batteryToGridWatts: p.BatteryToGridWatts,
                    batteryToGridWattEnergy: p.BatteryToGridWattEnergy,
                    gridToHomeWatts: p.GridToHomeWatts,
                    gridToHomeWattEnergy: p.GridToHomeWattEnergy,
                    gridToBatteryWatts: p.GridToBatteryWatts,
                    gridToBatteryWattEnergy: p.GridToBatteryWattEnergy,
                    secondsBatteryCharging: a.SecondsBatteryCharging,
                    secondsBatteryDischarging: a.SecondsBatteryDischarging,
                    secondsWithoutData: a.SecondsWithoutData)
            ).ToList();
            return response;
        }

        public async Task<IEnumerable<EnergySummaryReadModel>> Fetch(DateTime date)
        {
            var results = await FetchRaw(date);
            return ToReadModel(results);
        }

        public async Task<IEnumerable<EnergySummaryEntity>> FetchRaw(DateTime date)
        {
            var dateText = "ES_" + date.ToString("yyyy-MM-dd");
            var queryable = _readContext.GetQueryable<EnergySummaryEntity>();
            var iterator = queryable.Where(p => p.Partition.StartsWith(dateText) && p.Discriminator == EnergySummaryEntity.DISCRIMINATOR).ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            return results;
        }

        private static EnergySummaryReadModel[] ToReadModel(IEnumerable<EnergySummaryEntity> iterator)
        {
            var response = iterator.Select(a => new EnergySummaryReadModel(
                intervalEndExcluded: a.IntervalEndExcluded,
                intervalStartIncluded: a.IntervalStartIncluded,
                key: a.Id,
                version: a.Version,
                batteryPercentageFull: a.BatteryPercentageFull,
                gridExportWatts: a.PowerMovementSummary?.SolarToGridWatts ?? a.GridExportWatts,
                gridExportWattEnergy: a.PowerMovementSummary?.SolarToGridWattEnergy ?? a.GridExportWattEnergy,
                gridImportWatts: a.GridImportWatts,
                gridImportWattEnergy: a.GridImportWattEnergy,
                consumptionWatts: a.ConsumptionWatts,
                consumptionWattEnergy: a.ConsumptionWattEnergy,
                solarPowerGenerationWatts: a.SolarPowerGenerationWatts,
                solarPowerGenerationWattEnergy: a.SolarPowerGenerationWattEnergy,
                batteryChargeWatts: a.BatteryChargeWatts,
                batteryChargeWattEnergy: a.BatteryChargeWattEnergy,
                batteryDischargeWatts: a.BatteryDischargeWatts,
                batteryDischargeWattEnergy: a.BatteryDischargeWattEnergy,
                solarConsumption: a.PowerMovementSummary?.SolarToHomeWattEnergy,
                solarToCommunity: a.PowerMovementSummary?.SolarToGridWattEnergy,
                toHome: a.PowerMovementSummary?.ToHome,
                secondsBatteryCharging: a.SecondsBatteryCharging,
                secondsBatteryDischarging: a.SecondsBatteryDischarging,
                secondsWithoutData: a.SecondsWithoutData)
                ).ToArray();
            return response;
        }
    }
}
