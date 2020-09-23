using Entities;
using Newtonsoft.Json;
using System;

namespace Repository.Model
{
    public class EnergySummaryEntity : EnergySummary
    {
        public const string DISCRIMINATOR = "EnergySummary";
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        // required for documentDB
        public EnergySummaryEntity(): base(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default)
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
            estimateConsumptionWatts: entity.ConsumptionWatts,
            estimateConsumptionWattEnergy: entity.ConsumptionWattEnergy,
            batteryReportedSolarPowerGenerationWatts: entity.SolarPowerGenerationWatts,
            batteryReportedSolarPowerGenerationWattEnergy: entity.SolarPowerGenerationWattEnergy,
            batteryChargeWatts:entity.BatteryChargeWatts,
            batteryChargeWattEnergy:entity.BatteryChargeWattEnergy,
            batteryDischargeWatts:entity.BatteryDischargeWatts,
            batteryDischargeWattEnergy:entity.BatteryDischargeWattEnergy,
            solarInverterPowerGenerationWattEnergy: entity.SolarInverterPowerGenerationWattEnergy,
            solarInverterPowerGenerationWatts: entity.SolarInverterPowerGenerationWatts,
            powerMovementSummary: entity.PowerMovementSummary,
            secondsBatteryCharging:entity.SecondsBatteryCharging,
            secondsBatteryDischarging:entity.SecondsBatteryDischarging,
            secondsWithoutData:entity.SecondsWithoutData)
        {
            Id = this.GetKey();
            Partition = "ES_" + this.GetKey();
            Version = version;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Partition { get; set; }
        public string Discriminator { get => DISCRIMINATOR; }
        public int Version { get; set; }
    }
}
