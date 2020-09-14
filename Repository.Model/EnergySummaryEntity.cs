﻿using Entities;
using Newtonsoft.Json;
using System;

namespace Repository.Model
{
    public class EnergySummaryEntity : EnergySummary
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        // required for documentDB
        public EnergySummaryEntity(): base(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public EnergySummaryEntity(EnergySummary entity, int version) : base(
            intervalEndExcluded: entity.IntervalEndExcluded,
            intervalStartIncluded: entity.IntervalStartIncluded,
            batteryPercentageFull: entity.BatteryPercentageFull,
            gridExportWatts: entity.GridExportWatts,
            gridExportWattEnergy: entity.GridExportWattEnergy,
            gridImportWatts: entity.GridImportWatts,
            gridImportWattEnergy: entity.GridImportWattEnergy,
            consumptionWatts: entity.ConsumptionWatts,
            consumptionWattEnergy: entity.ConsumptionWattEnergy,
            solarPowerGenerationWatts:entity.SolarPowerGenerationWatts,
            solarPowerGenerationWattEnergy:entity.SolarPowerGenerationWattEnergy,
            batteryChargeWatts:entity.BatteryChargeWatts,
            batteryChargeWattEnergy:entity.BatteryChargeWattEnergy,
            batteryDischargeWatts:entity.BatteryDischargeWatts,
            batteryDischargeWattEnergy:entity.BatteryDischargeWattEnergy,
            secondsBatteryCharging:entity.SecondsBatteryCharging,
            secondsBatteryDischarging:entity.SecondsBatteryDischarging,
            secondsWithoutData:entity.SecondsWithoutData)
        {
            Id = this.GetKey();
            Partition = this.GetKey();
            Version = version;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Partition { get; set; }
        public string Discriminator { get => "EnergySummary"; }
        public int Version { get; set; }
    }
}
