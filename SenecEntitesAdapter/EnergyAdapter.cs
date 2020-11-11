using Entities;
using SenecEntitiesAdapter;
using System;
using SenecEnergy = SenecEntities.Energy;

namespace SenecEntitesAdapter
{
    public class EnergyAdapter : IEnergyAdapter
    {
        private readonly IAdapter _adapter;

        public EnergyAdapter(
            IAdapter adapter)
        {
            _adapter = adapter;
        }

        public Energy Convert(DateTimeOffset instant, SenecEnergy meter)
        {
            var result = new Energy(
                instant: instant,
                batteryPercentageFull: _adapter.GetDecimal(meter.GUI_BAT_DATA_FUEL_CHARGE),
                homeInstantPowerConsumption: _adapter.GetDecimal(meter.GUI_HOUSE_POW),
                solarPowerGeneration: _adapter.GetDecimal(meter.GUI_INVERTER_POWER),
                systemState: new SenecSystemState(_adapter.GetDecimal(meter.STAT_STATE)),
                isBatteryCharging: new SenecBoolean(_adapter.GetDecimal(meter.GUI_CHARGING_INFO)),
                isBatteryDischarging: new SenecBoolean(_adapter.GetDecimal(meter.GUI_BOOSTING_INFO)),
                gridImportWatts: _adapter.GetDecimal(meter.GUI_GRID_POW),
                batteryChargeWatts: _adapter.GetDecimal(meter.GUI_BAT_DATA_POWER),
                sTAT_MAINT_REQUIRED: _adapter.GetDecimal(meter.STAT_MAINT_REQUIRED),
                sTAT_STATE_DECODE: _adapter.GetDecimal(meter.STAT_STATE_DECODE)
                );

            return result;
        }
    }

    public interface IEnergyAdapter
    {
        Energy Convert(DateTimeOffset instant, SenecEnergy meter);
    }
}
