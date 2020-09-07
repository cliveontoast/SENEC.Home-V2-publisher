using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Energy
    {
        public Energy(DateTimeOffset instant, SenecDecimal batteryPercentageFull, SenecDecimal homeInstantPowerConsumption, SenecDecimal solarPowerGeneration, SenecSystemState systemState, SenecBoolean isBatteryDischarging, SenecBoolean isBatteryCharging, SenecDecimal gridImportWatts, SenecDecimal batteryChargeWatts, SenecDecimal sTAT_STATE_DECODE, SenecDecimal sTAT_MAINT_REQUIRED)
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
            BatteryChargeWatts = batteryChargeWatts;
            BatteryDischargeWatts = new SenecDecimal(BatteryChargeWatts.Type, BatteryChargeWatts.Value);
            MutualExclusive(BatteryChargeWatts, BatteryDischargeWatts);
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

        public SenecDecimal BatteryDischargeWatts { get; set; }
        public SenecDecimal BatteryChargeWatts { get; set; }
        public SenecDecimal SolarPowerGeneration { get; set; }
        public SenecBoolean IsBatteryDischarging { get; set; }
        public SenecBoolean IsBatteryCharging { get; set; }

        public MomentEnergy GetMomentEnergy()
        {
            if (BatteryPercentageFull.Value.HasValue
                && SystemState.HasValue
                && GridExportWatts.Value.HasValue
                && GridImportWatts.Value.HasValue
                && BatteryDischargeWatts.Value.HasValue
                && BatteryChargeWatts.Value.HasValue
                && IsBatteryDischarging.Value.HasValue
                && IsBatteryCharging.Value.HasValue)
            {
                return new MomentEnergy(
                    instant: Instant,
                    BatteryPercentageFull: BatteryPercentageFull.Value.Value,
                    SystemState: SystemState.EnglishName,
                    GridExportWatts: GridExportWatts.Value.Value,
                    GridImportWatts: GridImportWatts.Value.Value,
                    BatteryDischarge: BatteryDischargeWatts.Value.Value,
                    BatteryCharge: BatteryChargeWatts.Value.Value,
                    IsBatteryCharging: IsBatteryCharging.Value.Value,
                    IsBatteryDischarging: IsBatteryDischarging.Value.Value,
                    HomeInstantPowerConsumption: HomeInstantPowerConsumption.Value,
                    SolarPowerGeneration: SolarPowerGeneration.Value
                    );
            }
            return new MomentEnergy(Instant, SystemState.EnglishName);
        }

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
