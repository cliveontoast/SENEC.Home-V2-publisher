using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Energy
    {
        public DateTimeOffset Instant { get; set; }
        public SenecDecimal BatteryPercentageFull { get; set; }
        public SenecSystemState SystemState { get; set; }

        public SenecDecimal STAT_STATE_DECODE { get; set; } // always same as SystemState
        public SenecDecimal STAT_MAINT_REQUIRED { get; set; } // so far.. always zero

        public SenecDecimal HomeInstantPowerConsumption { get; set; }


        public SenecDecimal GridExportWatts { get; set; }
        public SenecDecimal GridImportWatts { get; set; }

        public SenecDecimal BatteryDischarge { get; set; }
        public SenecDecimal BatteryCharge { get; set; }
        public SenecDecimal SolarPowerGeneration { get; set; }
        public SenecBoolean IsBatteryDischarging { get; set; }
        public SenecBoolean IsBatteryCharging { get; set; }
    }
}
