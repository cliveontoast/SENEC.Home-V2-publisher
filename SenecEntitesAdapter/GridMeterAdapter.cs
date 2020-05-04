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
            var result = new Meter() { Instant = DateTimeOffset.FromUnixTimeSeconds(instant) };
            result.TotalPower = _adapter.GetDecimal(meter.P_TOTAL);
            result.Frequency = _adapter.GetDecimal(meter.FREQ);
            result.L1 = Convert(meter, 0);
            result.L2 = Convert(meter, 1);
            result.L3 = Convert(meter, 2);
            return result;
        }

        private MeterPhase Convert(SenecMeter meter, int phase)
        {
            return new MeterPhase
            {
                Current = _adapter.GetDecimal(meter.I_AC[phase]),
                Voltage = _adapter.GetDecimal(meter.U_AC[phase]),
                Power = _adapter.GetDecimal(meter.P_AC[phase]),
            };
        }
    }

    public interface IGridMeterAdapter
    {
        Meter Convert(long instant, SenecMeter meter);
    }
}
