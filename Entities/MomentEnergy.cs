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

        public MomentEnergy(DateTimeOffset instant, decimal BatteryPercentageFull, string SystemState, decimal GridExportWatts, decimal GridImportWatts, decimal BatteryDischarge, decimal BatteryCharge, bool IsBatteryCharging, bool IsBatteryDischarging, decimal? HomeInstantPowerConsumption, decimal? SolarPowerGeneration)
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
        public bool IsBatteryCharging { get; }
        public bool IsBatteryDischarging { get; }
        public decimal? HomeInstantPowerConsumption { get; }
        public decimal? SolarPowerGeneration { get; }
        public decimal? SolarInvertorsPowerGeneration { get; set; }
        public DateTimeOffset Instant { get; }
        public bool IsValid { get; }
        public PowerMovements? PowerMovements { get;set; }

        public decimal? AccurateHomeConsumption => PowerMovements?.HomeConsumption;
    }

    public class PowerMovements
    {
        public enum PowerStateEnum
        {
            SolarBatteryCharge,
            SolarBatteryDischarge,
            SolarBatteryDischargeUnderpowered,// solar + battery < grid export
            SolarBatteryIdle,
            SolarIdleBatteryCharge,
            SolarIdleBatteryDischarge,
            GridOnly,
            SolarBatteryChargeUnderpowered,
        }

        public decimal SolarPowerGeneration { get; set; }
        public decimal SolarToBattery { get; set; }
        public decimal SolarToGrid { get; set; }
        public decimal SolarToHome { get; set; }
        public decimal BatteryToHome { get; set; }
        public decimal BatteryToGrid { get; set; }
        public decimal GridToHome { get; set; }
        public decimal GridToBattery { get; set; }
        public PowerStateEnum PowerState { get; set; }
        public PowerStateEnum? CrapState { get; set; }

        public decimal HomeConsumption => BatteryToHome + GridToHome + SolarToHome;

        public PowerMovements(decimal gridToHome): this(gridToHome, PowerStateEnum.GridOnly)
        {
        }
        public PowerMovements(decimal gridToHome, PowerStateEnum powerState)
        {
            PowerState = powerState;
            GridToHome = gridToHome;
        }

        public PowerMovements(decimal batteryToGrid, decimal batteryToHome, decimal gridToHome)
            : this(gridToHome: gridToHome)
        {
            if (batteryToGrid > 0 || batteryToHome > 0)
                PowerState = PowerStateEnum.SolarIdleBatteryDischarge;
            BatteryToGrid = batteryToGrid;
            BatteryToHome = batteryToHome;
        }

        public PowerMovements(decimal gridToHome, decimal gridToBattery)
            : this(gridToHome: gridToHome)
        {
            if (gridToBattery > 0)
                PowerState = PowerStateEnum.SolarIdleBatteryCharge;
            GridToBattery = gridToBattery;
        }

        public PowerMovements(decimal solarToHome, decimal solarToGrid, PowerStateEnum powerState, decimal solarPower)
        {
            PowerState = powerState;
            SolarToHome = solarToHome;
            SolarToGrid = solarToGrid;
            SolarPowerGeneration = solarPower;
        }

        // todo make it better......
        public PowerMovements(decimal gridToHome, PowerStateEnum powerState, decimal solarToHome, bool isSolarPowerSame)
            : this(gridToHome: gridToHome, powerState: powerState)
        {
            // isSolarPowerSame always true
            SolarToHome = solarToHome;
            SolarPowerGeneration = solarToHome;
        }

        public PowerMovements(decimal solarToHome, decimal solarToGrid, PowerStateEnum powerState, decimal batteryToHome, decimal solarPower)
            : this(solarToHome: solarToHome, solarToGrid: solarToGrid, powerState: powerState, solarPower: solarPower)
        {
            BatteryToHome = batteryToHome;
            SolarPowerGeneration = solarPower;
        }

        public PowerMovements(decimal solarToHome, PowerStateEnum powerState, decimal batteryToGrid, decimal batteryToHome)
        {
            SolarToHome = solarToHome;
            PowerState = powerState;
            BatteryToGrid = batteryToGrid;
            BatteryToHome = batteryToHome;
        }

        public PowerMovements(PowerStateEnum powerState, decimal solarToGrid, decimal batteryToGrid, decimal batteryToHome, decimal solarPower)
        {
            PowerState = powerState;
            SolarToGrid = solarToGrid;
            BatteryToGrid = batteryToGrid;
            BatteryToHome = batteryToHome;
            SolarPowerGeneration = solarPower;
        }

        public PowerMovements(decimal solarToHome, decimal batteryToHome, decimal gridToHome, PowerStateEnum powerState, decimal solarPower)
        {
            SolarToHome = solarToHome;
            BatteryToHome = batteryToHome;
            GridToHome = gridToHome;
            PowerState = PowerStateEnum.SolarBatteryDischarge;
            CrapState = powerState;
            SolarPowerGeneration = solarPower;
        }

        public PowerMovements(decimal solarToBattery, decimal solarToGrid, decimal solarToHome, decimal gridToHome, PowerStateEnum powerState, decimal solarPower)
        {
            SolarToBattery = solarToBattery;
            SolarToGrid = solarToGrid;
            SolarToHome = solarToHome;
            GridToHome = gridToHome;
            PowerState = powerState;
            SolarPowerGeneration = solarPower;
        }

        public PowerMovements(decimal solarToBattery, int solarToHome, int solarToGrid, decimal gridToBattery, decimal gridToHome, PowerStateEnum powerState, decimal solarPower)
        {
            SolarToBattery = solarToBattery;
            SolarToHome = solarToHome;
            SolarToGrid = solarToGrid;
            GridToBattery = gridToBattery;
            GridToHome = gridToHome;
            PowerState = powerState;
            SolarPowerGeneration = solarPower;
        }
    }

    public class PowerMovementsBuilder
    {
        public static PowerMovements Build(MomentEnergy moment)
        {
            var solarPower = moment.SolarInvertorsPowerGeneration ?? moment.SolarPowerGeneration ?? 0;
            var isSolarPower = solarPower > 0;
            if (isSolarPower) {
                if (moment.IsBatteryCharging)
                    return SolarBatteryCharge(solarPower, moment);
                else if (moment.IsBatteryDischarging)
                    return SolarBatteryDischarge(solarPower, moment);
                else
                    return SolarBatteryIdle(solarPower, moment);
            }
            else { 
                if (moment.IsBatteryCharging)
                    return SolarIdleBatteryCharge(moment);
                else if (moment.IsBatteryDischarging)
                    return SolarIdleBatteryDischarge(moment);
                else
                    return GridOnly(moment);
            }
        }

        private static PowerMovements GridOnly(MomentEnergy moment)
        {
            return new PowerMovements(gridToHome: moment.GridImportWatts);
        }

        private static PowerMovements SolarIdleBatteryDischarge(MomentEnergy moment)
        {
            var batteryToGrid = moment.GridExportWatts;
            var batteryToHome = Math.Max(0, moment.BatteryDischarge - batteryToGrid);
            var gridToHome = moment.GridImportWatts;
            return new PowerMovements(
                batteryToGrid: batteryToGrid,
                batteryToHome: batteryToHome,
                gridToHome: gridToHome);
        }

        private static PowerMovements SolarIdleBatteryCharge(MomentEnergy moment)
        {
            var gridToBattery = moment.BatteryCharge;
            var gridToHome = Math.Max(0, moment.GridImportWatts - gridToBattery);
            return new PowerMovements(
                gridToHome: gridToHome,
                gridToBattery: gridToBattery);
        }

        private static PowerMovements SolarBatteryIdle(decimal solarPower, MomentEnergy moment)
        {
            if (moment.GridExportWatts > 0)
            {
                var solarToGrid = moment.GridExportWatts;
                var solarToHome = Math.Max(0, solarPower - solarToGrid);
                return new PowerMovements(
                    powerState: PowerMovements.PowerStateEnum.SolarBatteryIdle,
                    solarToHome: solarToHome,
                    solarToGrid: solarToGrid,
                    solarPower: solarPower);
            }
            else
            {
                var solarToHome = solarPower;
                return new PowerMovements(
                    gridToHome: moment.GridImportWatts,
                    powerState: PowerMovements.PowerStateEnum.SolarBatteryIdle,
                    solarToHome: solarToHome,
                    isSolarPowerSame: true
                    );
            }
        }

        private static PowerMovements SolarBatteryDischarge(decimal solarPower, MomentEnergy moment)
        {
            if (moment.GridExportWatts > 0)
            {
                if (solarPower > moment.GridExportWatts)
                {
                    // solar to grid
                    var solarToGrid = moment.GridExportWatts;
                    var solarToHome = solarPower - solarToGrid;
                    var batteryToHome = moment.BatteryDischarge;
                    return new PowerMovements(
                        solarToHome: solarToHome,
                        solarToGrid: solarToGrid,
                        powerState: PowerMovements.PowerStateEnum.SolarBatteryDischarge,
                        batteryToHome: batteryToHome,
                        solarPower: solarPower
                        );
                }
                else
                {
                    var solarToGrid = solarPower;
                    var batteryToGrid = moment.GridExportWatts - solarToGrid;
                    var batteryToHome = moment.BatteryDischarge - batteryToGrid;
                    if (batteryToHome < 0)
                    {
                        return new PowerMovements(
                            powerState: PowerMovements.PowerStateEnum.SolarBatteryDischargeUnderpowered,
                            solarToGrid: solarToGrid,
                            batteryToGrid: batteryToGrid,
                            batteryToHome: batteryToHome,
                            solarPower: solarPower);
                    }
                    else
                    {
                        return new PowerMovements(
                            powerState: PowerMovements.PowerStateEnum.SolarBatteryDischarge,
                            solarToGrid: solarToGrid,
                            batteryToGrid: batteryToGrid,
                            batteryToHome: batteryToHome,
                            solarPower: solarPower);
                    }
                }
            }
            else 
            {
                var solarToHome = solarPower;
                var batteryToHome = moment.BatteryDischarge;
                var gridToHome = moment.GridImportWatts; // can be 0
                return new PowerMovements(
                    solarToHome: solarToHome,
                    batteryToHome: batteryToHome,
                    gridToHome: gridToHome,
                    powerState: PowerMovements.PowerStateEnum.SolarBatteryDischarge,
                    solarPower: solarPower);
            }
        }

        private static PowerMovements SolarBatteryCharge(decimal solarPower, MomentEnergy moment)
        {
            if (moment.GridExportWatts >= 0 && moment.GridImportWatts == 0)
            {
                //var gridToBattery = 0;
                //var gridToHome = 0;
                var solarToBattery = Math.Min(solarPower, moment.BatteryCharge);
                var solarToGrid = moment.GridExportWatts;
                var solarToHome = solarPower - solarToBattery - solarToGrid;
                var powerState = solarToHome < 0 ? PowerMovements.PowerStateEnum.SolarBatteryChargeUnderpowered : PowerMovements.PowerStateEnum.SolarBatteryCharge;
                return new PowerMovements(
                    solarToBattery: solarToBattery,
                    solarToGrid: solarToGrid,
                    solarToHome: solarToHome,
                    gridToHome: 0,
                    powerState: powerState,
                    solarPower: solarPower);
            }
            else // grid import > 0
            {
                PowerMovements? result = null;
                if (solarPower > moment.BatteryCharge)
                {
                    var solarToBattery = moment.BatteryCharge;
                    var solarToHome = solarPower - solarToBattery;
                    var solarToGrid = 0;
                    var gridToHome = moment.GridImportWatts;
                    result = new PowerMovements(
                        solarToBattery: solarToBattery,
                        solarToGrid: solarToGrid,
                        solarToHome: solarToHome,
                        gridToHome: gridToHome,
                        powerState: PowerMovements.PowerStateEnum.SolarBatteryCharge,
                        solarPower: solarPower
                        );
                }
                else // solar <= battery charge
                {
                    var solarToBattery = solarPower;
                    var solarToHome = 0;
                    var solarToGrid = 0;
                    var gridToBattery = moment.BatteryCharge - solarToBattery;
                    var gridToHome = moment.GridImportWatts - gridToBattery;
                    result = new PowerMovements(
                        solarToBattery: solarToBattery,
                        solarToHome: solarToHome,
                        solarToGrid: solarToGrid,
                        gridToBattery: gridToBattery,
                        gridToHome: gridToHome,
                        powerState: PowerMovements.PowerStateEnum.SolarBatteryCharge,
                        solarPower: solarPower
                        );
                }
                CheckIfBatteryCausedGridImport(result);
                return result;
            }
        }

        private static void CheckIfBatteryCausedGridImport(PowerMovements result)
        {
            if (result.SolarToBattery > 0 && result.GridToHome > 0)
            {
                var powerToMove = Math.Min(result.SolarToBattery, result.GridToHome);
                result.GridToHome -= powerToMove;
                result.GridToBattery += powerToMove;

                result.SolarToBattery -= powerToMove;
                result.SolarToHome += powerToMove;
            }
        }
    }

}
