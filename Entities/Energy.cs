using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Energy
    {
        public Energy(DateTimeOffset instant, SenecDecimal batteryPercentageFull, SenecDecimal homeInstantPowerConsumption, SenecDecimal solarPowerGeneration, SenecSystemState systemState, SenecBoolean isBatteryDischarging, SenecBoolean isBatteryCharging, SenecDecimal gridImportWatts, SenecDecimal batteryCharge, SenecDecimal sTAT_STATE_DECODE, SenecDecimal sTAT_MAINT_REQUIRED)
        {
            Instant = instant;
            BatteryPercentageFull = batteryPercentageFull;
            HomeInstantPowerConsumption = homeInstantPowerConsumption;
            SolarPowerGeneration = solarPowerGeneration;
            SystemState = systemState;
            IsBatteryDischarging = isBatteryDischarging;
            IsBatteryCharging = isBatteryCharging;
            GridImportWatts = gridImportWatts;
            GridExportWatts = new SenecDecimal(GridImportWatts.Type, GridImportWatts.Value);
            MutualExclusive(GridImportWatts, GridExportWatts);
            BatteryCharge = batteryCharge;
            BatteryDischarge = new SenecDecimal(BatteryCharge.Type, BatteryCharge.Value);
            MutualExclusive(BatteryCharge, BatteryDischarge);
            STAT_STATE_DECODE = sTAT_STATE_DECODE;
            STAT_MAINT_REQUIRED = sTAT_MAINT_REQUIRED;
        }

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

        private static void MutualExclusive(SenecDecimal leftItem, SenecDecimal rightItem)
        {
            if (leftItem.Value.HasValue)
            {
                if (leftItem.Value.Value > decimal.Zero)
                {
                    rightItem.Value = 0;
                }
                else
                {
                    leftItem.Value = 0;
                    rightItem.Value = -rightItem.Value;
                }
            }
        }
    }
}
