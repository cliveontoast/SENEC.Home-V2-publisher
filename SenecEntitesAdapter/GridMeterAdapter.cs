using Entities;
using SenecEntitiesAdapter;
using System;
using SenecMeter = SenecEntities.Meter;

namespace SenecEntitesAdapter
{
    public class GridMeterAdapter : IGridMeterAdapter
    {
        private readonly IAdapter _adapter;

        public GridMeterAdapter(
            IAdapter adapter)
        {
            _adapter = adapter;
        }


        public Meter Convert(long instant, SenecMeter meter)
        {
            if (meter == null) return null;
            var result = new Meter(
                instant: DateTimeOffset.FromUnixTimeSeconds(instant),
                totalPower: _adapter.GetDecimal(meter.P_TOTAL),
                frequency: _adapter.GetDecimal(meter.FREQ),
                l1: Convert(meter, 0),
                l2: Convert(meter, 1),
                l3: Convert(meter, 2)
                );
            return result;
        }

        private MeterPhase Convert(SenecMeter meter, int phase)
        {
            return new MeterPhase(
                current: _adapter.GetDecimal(meter.I_AC[phase]),
                voltage: _adapter.GetDecimal(meter.U_AC[phase]),
                power: _adapter.GetDecimal(meter.P_AC[phase])
            );
        }
    }

    public interface IGridMeterAdapter
    {
        Meter Convert(long instant, SenecMeter meter);
    }
}
