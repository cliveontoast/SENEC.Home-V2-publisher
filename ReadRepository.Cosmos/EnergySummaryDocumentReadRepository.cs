﻿using Microsoft.Azure.Cosmos.Linq;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
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

        public async Task<EnergySummaryReadModel> Get(string key, CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<EnergySummaryEntity>();
            var iterator = queryable.Where(p => p.Partition == "ES_" + key).ToFeedIterator();
            var response = await ToReadModel(iterator);
            return response.FirstOrDefault();
        }
        public async Task<IEnumerable<EnergySummaryReadModel>> Fetch(DateTime date)
        {
            var dateText = "ES_" + date.ToString("yyyy-MM-dd");
            var queryable = _readContext.GetQueryable<EnergySummaryEntity>();
            var iterator = queryable.Where(p => p.Partition.StartsWith(dateText) && p.Discriminator == EnergySummaryEntity.DISCRIMINATOR).ToFeedIterator();
            var response = await ToReadModel(iterator);
            return response;
        }

        private static async Task<ImmutableList<EnergySummaryReadModel>> ToReadModel(Microsoft.Azure.Cosmos.FeedIterator<EnergySummaryEntity> iterator)
        {
            var results = await iterator.ReadNextAsync();
            var response = results.Select(a => new EnergySummaryReadModel(
                intervalEndExcluded: a.IntervalEndExcluded,
                intervalStartIncluded: a.IntervalStartIncluded,
                key: a.Id,
                version: a.Version,
                batteryPercentageFull: a.BatteryPercentageFull,
                gridExportWatts: a.GridExportWatts,
                gridExportWattEnergy: a.GridExportWattEnergy,
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
                secondsBatteryCharging: a.SecondsBatteryCharging,
                secondsBatteryDischarging: a.SecondsBatteryDischarging ,
                secondsWithoutData: a.SecondsWithoutData)
                ).ToImmutableList();
            return response;
        }
    }
}
