using Entities;
using System;

namespace ReadRepository.ReadModel
{
    public class PowerMovementSummaryReadModel
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public Statistic SolarToGridWatts { get; }
        public decimal SolarToGridWattHours { get; }
        public Statistic SolarToBatteryWatts { get; }
        public decimal SolarToBatteryWattHours { get; }
        public Statistic SolarToHomeWatts { get; }
        public decimal SolarToHomeWattHours { get; }
        public Statistic BatteryToHomeWatts { get; }
        public decimal BatteryToHomeWattHours { get; }
        public Statistic BatteryToGridWatts { get; }
        public decimal BatteryToGridWattHours { get; }
        public Statistic GridToHomeWatts { get; }
        public decimal GridToHomeWattHours { get; }
        public Statistic GridToBatteryWatts { get; }
        public decimal GridToBatteryWattHours { get; }
        public string Key { get; set; }

        public int SecondsBatteryCharging { get; set; }
        public int SecondsBatteryDischarging { get; set; }
        public int SecondsWithoutData { get; set; }
        public decimal ConsumptionWattHours => SolarToHomeWattHours + GridToHomeWattHours + BatteryToHomeWattHours;

        public PowerMovementSummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, int version,
            Statistic solarToGridWatts, decimal solarToGridWattEnergy,
            Statistic solarToBatteryWatts, decimal solarToBatteryWattEnergy,
            Statistic solarToHomeWatts, decimal solarToHomeWattEnergy,
            Statistic batteryToHomeWatts, decimal batteryToHomeWattEnergy,
            Statistic batteryToGridWatts, decimal batteryToGridWattEnergy,
            Statistic gridToHomeWatts, decimal gridToHomeWattEnergy,
            Statistic gridToBatteryWatts, decimal gridToBatteryWattEnergy,
            int secondsBatteryCharging, int secondsBatteryDischarging,
            int secondsWithoutData)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Version = version;
            SolarToGridWatts = solarToGridWatts;
            SolarToGridWattHours = AsWattHours(solarToGridWattEnergy);
            SolarToBatteryWatts = solarToBatteryWatts;
            SolarToBatteryWattHours = AsWattHours(solarToBatteryWattEnergy);
            SolarToHomeWatts = solarToHomeWatts;
            SolarToHomeWattHours = AsWattHours(solarToHomeWattEnergy);
            BatteryToHomeWatts = batteryToHomeWatts;
            BatteryToHomeWattHours = AsWattHours(batteryToHomeWattEnergy);
            BatteryToGridWatts = batteryToGridWatts;
            BatteryToGridWattHours = AsWattHours(batteryToGridWattEnergy);
            GridToHomeWatts = gridToHomeWatts;
            GridToHomeWattHours = AsWattHours(gridToHomeWattEnergy);
            GridToBatteryWatts = gridToBatteryWatts;
            GridToBatteryWattHours = AsWattHours(gridToBatteryWattEnergy);
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
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
