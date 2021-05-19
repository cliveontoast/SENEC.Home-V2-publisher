using Entities;
using SenecEntities;
using SenecEntitiesAdapter;
using System;
using System.Collections.Generic;
using SenecBatteryObj = SenecEntities.BatteryObject1;

namespace SenecEntitesAdapter
{
    public class BatteryTemperatureAdapter : IBatteryTemperatureAdapter
    {
        private readonly IAdapter _adapter;

        public BatteryTemperatureAdapter(
            IAdapter adapter)
        {
            _adapter = adapter;
        }

        public InverterTemperatures Convert(DateTimeOffset instant, Temperatures source)
        {
            var result = new InverterTemperatures(
                instant: instant,
                batteryCelsius: _adapter.GetDecimal(source.TemperatureMeasurements.BATTERY_TEMP),
                caseCelsius: _adapter.GetDecimal(source.TemperatureMeasurements.CASE_TEMP),
                temperatures: new List<SenecDecimal>
                {
                    _adapter.GetDecimal(source.Inverter.TEMP1),
                    _adapter.GetDecimal(source.Inverter.TEMP2),
                    _adapter.GetDecimal(source.Inverter.TEMP3),
                    _adapter.GetDecimal(source.Inverter.TEMP4),
                    _adapter.GetDecimal(source.Inverter.TEMP5),
                });

            return result;
        }
    }

    public interface IBatteryTemperatureAdapter
    {
        InverterTemperatures Convert(DateTimeOffset instant, Temperatures meter);
    }
}
