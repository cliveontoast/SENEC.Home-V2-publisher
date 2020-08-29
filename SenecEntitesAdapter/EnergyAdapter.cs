using Entities;
using SenecEntitiesAdapter;
using System;
using System.ComponentModel;
using SenecEnergy = SenecEntities.Energy;

namespace SenecEntitesAdapter
{
    public class EnergyAdapter : IEvergyAdapter
    {
        private readonly IAdapter _adapter;

        public EnergyAdapter(
            IAdapter adapter)
        {
            _adapter = adapter;
        }

        public Entities.Energy Convert(long instant, SenecEnergy meter)
        {
            if (meter == null) return null;
            var result = new Entities.Energy() { Instant = DateTimeOffset.FromUnixTimeSeconds(instant) };
            result.BatteryPercentageFull = _adapter.GetDecimal(meter.GUI_BAT_DATA_FUEL_CHARGE);
            result.HomeInstantPowerConsumption = _adapter.GetDecimal(meter.GUI_HOUSE_POW);
            result.GridImportKiloWatts = _adapter.GetDecimal(meter.GUI_GRID_POW);
            result.GridExportKiloWatts = _adapter.GetDecimal(meter.GUI_GRID_POW);
            if (result.GridImportKiloWatts.Value.HasValue)
            {
                if (result.GridImportKiloWatts.Value.Value > decimal.Zero)
                {
                    result.GridExportKiloWatts.Value = 0;
                }
                else
                {
                    result.GridImportKiloWatts.Value = 0;
                    result.GridExportKiloWatts.Value = -result.GridExportKiloWatts.Value;
                }
            }

            result.BatteryDischarge = _adapter.GetDecimal(meter.GUI_BAT_DATA_POWER);
            result.BatteryCharge = _adapter.GetDecimal(meter.GUI_BAT_DATA_POWER);
            if (result.GridImportKiloWatts.Value.HasValue)
            {
                if (result.BatteryCharge.Value.Value > decimal.Zero)
                {
                    result.BatteryDischarge.Value = 0;
                }
                else
                {
                    result.BatteryCharge.Value = 0;
                    result.BatteryDischarge.Value = -result.BatteryDischarge.Value;
                }
            }
            result.SolarPowerGeneration = _adapter.GetDecimal(meter.GUI_INVERTER_POWER);


            result.GUI_BOOSTING_INFO = _adapter.GetDecimal(meter.GUI_BOOSTING_INFO);
            result.GUI_CHARGING_INFO = _adapter.GetDecimal(meter.GUI_CHARGING_INFO);
            result.STAT_MAINT_REQUIRED = _adapter.GetDecimal(meter.STAT_MAINT_REQUIRED);
            result.SystemState = new SenecSystemState(_adapter.GetDecimal(meter.STAT_STATE));
            result.STAT_STATE_DECODE = _adapter.GetDecimal(meter.STAT_STATE_DECODE);

            return result;
        }
    }

    public interface IEvergyAdapter
    {
        Entities.Energy Convert(long instant, SenecEnergy meter);
    }
}
