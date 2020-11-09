using Entities;
using System;

namespace ReadRepository.ReadModel
{
    public class EnergySummaryReadModel : IRepositoryReadModel
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }

        #region Available on website
        public Statistic GridExportWatts { get; set; }
        public decimal GridExportWattHours { get; set; }
        public Statistic GridImportWatts { get; set; }
        public decimal GridImportWattHours { get; set; }
        public Statistic ConsumptionWatts { get; set; }
        public decimal ConsumptionWattHours { get; set; }
        public Statistic SolarPowerGenerationWatts { get; set; }
        public decimal SolarPowerGenerationWattHours { get; set; }
        public Statistic BatteryChargeWatts { get; set; }
        public decimal BatteryChargeWattHours { get; set; }
        public Statistic BatteryDischargeWatts { get; set; }
        public decimal BatteryDischargeWattHours { get; set; }
        #endregion

        #region Unavailable on website
        public Statistic BatteryPercentageFull { get; set; }
        public int SecondsBatteryCharging { get; set; }
        public int SecondsBatteryDischarging { get; set; }
        #endregion

        public int SecondsWithoutData { get; set; }

        public EnergySummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, int version,
            Statistic batteryPercentageFull,
            Statistic gridExportWatts, decimal gridExportWattEnergy,
            Statistic gridImportWatts, decimal gridImportWattEnergy,
            Statistic consumptionWatts, decimal consumptionWattEnergy,
            Statistic solarPowerGenerationWatts, decimal solarPowerGenerationWattEnergy,
            Statistic batteryChargeWatts, decimal batteryChargeWattEnergy,
            Statistic batteryDischargeWatts, decimal batteryDischargeWattEnergy,
            int secondsBatteryCharging, int secondsBatteryDischarging,
            int secondsWithoutData)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Version = version;

            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            BatteryPercentageFull = batteryPercentageFull;
            GridExportWatts = gridExportWatts;
            GridExportWattHours = AsWattHours(gridExportWattEnergy);
            GridImportWatts = gridImportWatts;
            GridImportWattHours = AsWattHours(gridImportWattEnergy);
            ConsumptionWatts = consumptionWatts;
            ConsumptionWattHours = AsWattHours(consumptionWattEnergy);
            SolarPowerGenerationWatts = solarPowerGenerationWatts;
            SolarPowerGenerationWattHours = AsWattHours(solarPowerGenerationWattEnergy);
            BatteryChargeWatts = batteryChargeWatts;
            BatteryChargeWattHours = AsWattHours(batteryChargeWattEnergy);
            BatteryDischargeWatts = batteryDischargeWatts;
            BatteryDischargeWattHours = AsWattHours(batteryDischargeWattEnergy);
            SecondsBatteryCharging = secondsBatteryCharging;
            SecondsBatteryDischarging = secondsBatteryDischarging;
            SecondsWithoutData = secondsWithoutData;
        }

        private decimal AsWattHours(decimal energy, int decimalPlaces = 0)
        {
            var totalSeconds = (IntervalEndExcluded - IntervalStartIncluded).TotalSeconds;
            if (totalSeconds > 0)
            {
                var result = decimal.Round(energy / new decimal(totalSeconds), decimalPlaces);
                return result;
            }
            return energy;
        }
    }
}
