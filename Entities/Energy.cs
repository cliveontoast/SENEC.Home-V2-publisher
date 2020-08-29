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
        public SenecDecimal GUI_CHARGING_INFO { get; set; } // guess 0 to 5? to display the flashing icon on website
        public SenecDecimal GUI_BOOSTING_INFO { get; set; } // guess 0 to 5? to display the flashing icon on website

        public SenecDecimal HomeInstantPowerConsumption { get; set; }


        public SenecDecimal GridExportKiloWatts { get; set; }
        public SenecDecimal GridImportKiloWatts { get; set; }

        public SenecDecimal BatteryDischarge { get; set; }
        public SenecDecimal BatteryCharge { get; set; }
        public SenecDecimal SolarPowerGeneration { get; set; }
    }
}
