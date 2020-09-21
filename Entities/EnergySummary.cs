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
        public Statistic SolarInverterPowerGenerationWatts { get; }
        public decimal SolarInverterPowerGenerationWattEnergy { get; }
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
            Statistic solarInverterPowerGenerationWatts, decimal solarInverterPowerGenerationWattEnergy)
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

            // solar to home
            // solar to battery
            // solar to grid
            // battery to home
            // battery to grid
            // grid to home
            // grid to battery
        }
    }
}
