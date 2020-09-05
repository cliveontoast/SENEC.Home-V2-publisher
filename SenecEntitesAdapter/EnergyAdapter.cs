﻿using Entities;
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

        public Energy Convert(long instant, SenecEnergy meter)
        {
            var result = new Energy(
                instant: DateTimeOffset.FromUnixTimeSeconds(instant),
                batteryPercentageFull: _adapter.GetDecimal(meter.GUI_BAT_DATA_FUEL_CHARGE),
                homeInstantPowerConsumption: _adapter.GetDecimal(meter.GUI_HOUSE_POW),
                solarPowerGeneration: _adapter.GetDecimal(meter.GUI_INVERTER_POWER),
                systemState: new SenecSystemState(_adapter.GetDecimal(meter.STAT_STATE)),
                isBatteryCharging: new SenecBoolean(_adapter.GetDecimal(meter.GUI_CHARGING_INFO)),
                isBatteryDischarging: new SenecBoolean(_adapter.GetDecimal(meter.GUI_BOOSTING_INFO)),
                gridImportWatts: _adapter.GetDecimal(meter.GUI_GRID_POW),
                batteryCharge: _adapter.GetDecimal(meter.GUI_BAT_DATA_POWER),
                sTAT_MAINT_REQUIRED: _adapter.GetDecimal(meter.STAT_MAINT_REQUIRED),
                sTAT_STATE_DECODE: _adapter.GetDecimal(meter.STAT_STATE_DECODE)
                );

            return result;
        }
    }

    public interface IEvergyAdapter
    {
        Energy Convert(long instant, SenecEnergy meter);
    }
}
