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
            var result = new Energy() { Instant = DateTimeOffset.FromUnixTimeSeconds(instant) };
            result.BatteryPercentageFull = _adapter.GetDecimal(meter.GUI_BAT_DATA_FUEL_CHARGE);
            result.HomeInstantPowerConsumption = _adapter.GetDecimal(meter.GUI_HOUSE_POW);
            result.SolarPowerGeneration = _adapter.GetDecimal(meter.GUI_INVERTER_POWER);
            result.SystemState = new SenecSystemState(_adapter.GetDecimal(meter.STAT_STATE));
            result.IsBatteryDischarging = new SenecBoolean(_adapter.GetDecimal(meter.GUI_BOOSTING_INFO));
            result.IsBatteryCharging = new SenecBoolean(_adapter.GetDecimal(meter.GUI_CHARGING_INFO));

            result.GridImportWatts = _adapter.GetDecimal(meter.GUI_GRID_POW);
            result.GridExportWatts = _adapter.GetDecimal(meter.GUI_GRID_POW);
            MutualExclusive(result.GridImportWatts, result.GridExportWatts);

            result.BatteryDischarge = _adapter.GetDecimal(meter.GUI_BAT_DATA_POWER);
            result.BatteryCharge = _adapter.GetDecimal(meter.GUI_BAT_DATA_POWER);
            MutualExclusive(result.BatteryCharge, result.BatteryDischarge);

            // unknown values
            result.STAT_MAINT_REQUIRED = _adapter.GetDecimal(meter.STAT_MAINT_REQUIRED);
            result.STAT_STATE_DECODE = _adapter.GetDecimal(meter.STAT_STATE_DECODE);

            return result;
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

    public interface IEvergyAdapter
    {
        Entities.Energy Convert(long instant, SenecEnergy meter);
    }
}
