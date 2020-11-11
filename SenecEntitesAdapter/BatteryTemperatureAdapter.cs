using Entities;
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

        public InverterTemperatures Convert(DateTimeOffset instant, SenecBatteryObj source)
        {
            var result = new InverterTemperatures(
                instant: instant,
                temperatures: new List<SenecDecimal>
                {
                    _adapter.GetDecimal(source.TEMP1),
                    _adapter.GetDecimal(source.TEMP2),
                    _adapter.GetDecimal(source.TEMP3),
                    _adapter.GetDecimal(source.TEMP4),
                    _adapter.GetDecimal(source.TEMP5),
                });

            return result;
        }
    }

    public interface IBatteryTemperatureAdapter
    {
        InverterTemperatures Convert(DateTimeOffset instant, SenecBatteryObj meter);
    }
}
