using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class EnergySummary
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }

        #region Available on website
        public decimal GridExportWattHours { get; set; }
        public decimal GridImportWattHours { get; set; }

        public decimal ConsumptionWattHours { get; set; }
        public decimal SolarPowerGenerationWattHours { get; set; }

        public decimal BatteryChargeWattHours { get; set; }
        public decimal BatteryDischargeWattHours { get; set; }
        #endregion

        #region Unavailable on website
        public decimal BatteryPercentageFull { get; set; }
        public int SecondsBatteryCharging { get; set; }
        public int SecondsBatteryDiscarging { get; set; }
        #endregion 

        public int SecondsWithoutData { get; set; }

        public EnergySummary(
            DateTimeOffset intervalStartIncluded,
            DateTimeOffset intervalEndExcluded,
            decimal batteryPercentageFull, decimal gridExportWattHours, decimal gridImportWattHours, decimal consumptionWattHours, decimal solarPowerGenerationWattHours, decimal batteryChargeWattHours, decimal batteryDischargeWattHours, int secondsBatteryCharging, int secondsBatteryDiscarging, int secondsWithoutData)
        {
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            BatteryPercentageFull = batteryPercentageFull;
            GridExportWattHours = gridExportWattHours;
            GridImportWattHours = gridImportWattHours;
            ConsumptionWattHours = consumptionWattHours;
            SolarPowerGenerationWattHours = solarPowerGenerationWattHours;
            BatteryChargeWattHours = batteryChargeWattHours;
            BatteryDischargeWattHours = batteryDischargeWattHours;
            SecondsBatteryCharging = secondsBatteryCharging;
            SecondsBatteryDiscarging = secondsBatteryDiscarging;
            SecondsWithoutData = secondsWithoutData;
        }
    }
}
