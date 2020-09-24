using MediatR;
using System;

namespace Entities
{
    public class EnergySummary : INotification
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }

        #region Available on website
        public Statistic GridExportWatts { get; set; }
        public decimal GridExportWattEnergy { get; set; }
        public Statistic GridImportWatts { get; set; }
        public decimal GridImportWattEnergy { get; set; }
        public Statistic ConsumptionWatts { get; set; }
        public decimal ConsumptionWattEnergy { get; set; }
        public Statistic SolarPowerGenerationWatts { get; set; }
        public decimal SolarPowerGenerationWattEnergy { get; set; }
        public Statistic BatteryChargeWatts { get; set; }
        public decimal BatteryChargeWattEnergy { get; set; }
        public Statistic BatteryDischargeWatts { get; set; }
        public decimal BatteryDischargeWattEnergy { get; set; }
        #endregion

        #region Unavailable on website
        public Statistic BatteryPercentageFull { get; set; }
        public int SecondsBatteryCharging { get; set; }
        public int SecondsBatteryDischarging { get; set; }
        public int SecondsWithoutData { get; set; }
        public Statistic SolarInverterPowerGenerationWatts { get; set; }
        public decimal SolarInverterPowerGenerationWattEnergy { get; set; }
        public PowerMovementSummary? PowerMovementSummary { get; set; } // version 3+

        #endregion


        public EnergySummary(
            DateTimeOffset intervalStartIncluded, DateTimeOffset intervalEndExcluded,
            Statistic batteryPercentageFull, 
            Statistic gridExportWatts, decimal gridExportWattEnergy,
            Statistic gridImportWatts, decimal gridImportWattEnergy,
            Statistic estimateConsumptionWatts, decimal estimateConsumptionWattEnergy,
            Statistic batteryReportedSolarPowerGenerationWatts, decimal batteryReportedSolarPowerGenerationWattEnergy,
            Statistic batteryChargeWatts, decimal batteryChargeWattEnergy,
            Statistic batteryDischargeWatts, decimal batteryDischargeWattEnergy,
            int secondsBatteryCharging, int secondsBatteryDischarging, int secondsWithoutData,
            Statistic solarInverterPowerGenerationWatts, decimal solarInverterPowerGenerationWattEnergy,
            PowerMovementSummary? powerMovementSummary
            )
        {
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            BatteryPercentageFull = batteryPercentageFull;
            GridExportWatts = gridExportWatts;
            GridExportWattEnergy = gridExportWattEnergy;
            GridImportWatts = gridImportWatts;
            GridImportWattEnergy = gridImportWattEnergy;
            ConsumptionWatts = estimateConsumptionWatts;
            ConsumptionWattEnergy = estimateConsumptionWattEnergy;
            SolarPowerGenerationWatts = batteryReportedSolarPowerGenerationWatts;
            SolarPowerGenerationWattEnergy = batteryReportedSolarPowerGenerationWattEnergy;
            BatteryChargeWatts = batteryChargeWatts;
            BatteryChargeWattEnergy = batteryChargeWattEnergy;
            BatteryDischargeWatts = batteryDischargeWatts;
            BatteryDischargeWattEnergy = batteryDischargeWattEnergy;
            SecondsBatteryCharging = secondsBatteryCharging;
            SecondsBatteryDischarging = secondsBatteryDischarging;
            SecondsWithoutData = secondsWithoutData;
            SolarInverterPowerGenerationWatts = solarInverterPowerGenerationWatts;
            SolarInverterPowerGenerationWattEnergy = solarInverterPowerGenerationWattEnergy;
            PowerMovementSummary = powerMovementSummary;
        }
    }

    public class PowerMovementSummary
    {
        // required for documentDB
        public PowerMovementSummary() : this(default, default, default, default, default, default, default, default, default, default, default, default, default, default)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public PowerMovementSummary(
            Statistic solarToGridWatts, decimal solarToGridWattEnergy,
            Statistic solarToBatteryWatts, decimal solarToBatteryWattEnergy,
            Statistic solarToHomeWatts, decimal solarToHomeWattEnergy,
            Statistic batteryToHomeWatts, decimal batteryToHomeWattEnergy,
            Statistic batteryToGridWatts, decimal batteryToGridWattEnergy,
            Statistic gridToHomeWatts, decimal gridToHomeWattEnergy,
            Statistic gridToBatteryWatts, decimal gridToBatteryWattEnergy)
        {
            SolarToGridWatts = solarToGridWatts;
            SolarToGridWattEnergy = solarToGridWattEnergy;
            SolarToBatteryWatts = solarToBatteryWatts;
            SolarToBatteryWattEnergy = solarToBatteryWattEnergy;
            SolarToHomeWatts = solarToHomeWatts;
            SolarToHomeWattEnergy = solarToHomeWattEnergy;
            BatteryToHomeWatts = batteryToHomeWatts;
            BatteryToHomeWattEnergy = batteryToHomeWattEnergy;
            BatteryToGridWatts = batteryToGridWatts;
            BatteryToGridWattEnergy = batteryToGridWattEnergy;
            GridToHomeWatts = gridToHomeWatts;
            GridToHomeWattEnergy = gridToHomeWattEnergy;
            GridToBatteryWatts = gridToBatteryWatts;
            GridToBatteryWattEnergy = gridToBatteryWattEnergy;
        }

        public Statistic SolarToGridWatts { get; set; }
        public decimal SolarToGridWattEnergy { get; set; }
        public Statistic SolarToBatteryWatts { get; set; }
        public decimal SolarToBatteryWattEnergy { get; set; }
        public Statistic SolarToHomeWatts { get; set; }
        public decimal SolarToHomeWattEnergy { get; set; }
        public Statistic BatteryToHomeWatts { get; set; }
        public decimal BatteryToHomeWattEnergy { get; set; }
        public Statistic BatteryToGridWatts { get; set; }
        public decimal BatteryToGridWattEnergy { get; set; }
        public Statistic GridToHomeWatts { get; set; }
        public decimal GridToHomeWattEnergy { get; set; }
        public Statistic GridToBatteryWatts { get; set; }
        public decimal GridToBatteryWattEnergy { get; set; }
    }
}
