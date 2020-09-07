using System;

namespace Entities
{
    public class MomentEnergy
    {
        public MomentEnergy(DateTimeOffset instant, string systemState)
        {
            Instant = instant;
            IsValid = false;
            SystemState = systemState ?? "";
        }

        public MomentEnergy(DateTimeOffset instant, decimal BatteryPercentageFull, string SystemState, decimal GridExportWatts, decimal GridImportWatts, decimal BatteryDischarge, decimal BatteryCharge, decimal IsBatteryCharging, decimal IsBatteryDischarging, decimal? HomeInstantPowerConsumption, decimal? SolarPowerGeneration)
            : this(instant, SystemState)
        {
            this.BatteryPercentageFull = BatteryPercentageFull;
            this.SystemState = SystemState;
            this.GridExportWatts = GridExportWatts;
            this.GridImportWatts = GridImportWatts;
            this.BatteryDischarge = BatteryDischarge;
            this.BatteryCharge = BatteryCharge;
            this.IsBatteryCharging = IsBatteryCharging;
            this.IsBatteryDischarging = IsBatteryDischarging;
            this.HomeInstantPowerConsumption = HomeInstantPowerConsumption;
            this.SolarPowerGeneration = SolarPowerGeneration;
            this.Instant = instant;
            this.IsValid = true;
        }

        public decimal BatteryPercentageFull { get; }
        public string SystemState { get; }
        public decimal GridExportWatts { get; }
        public decimal GridImportWatts { get; }
        public decimal BatteryDischarge { get; }
        public decimal BatteryCharge { get; }
        public decimal IsBatteryCharging { get; }
        public decimal IsBatteryDischarging { get; }
        public decimal? HomeInstantPowerConsumption { get; }
        public decimal? SolarPowerGeneration { get; }
        public DateTimeOffset Instant { get; }
        public bool IsValid { get; }
    }
}
