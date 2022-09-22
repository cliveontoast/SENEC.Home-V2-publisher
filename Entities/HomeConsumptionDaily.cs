using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class HomeConsumptionDaily
    {
        public DateTime Date { get; set; }
        public DayConsumptionSource BatteryCharge { get; }
        public DayConsumptionSource SolarExported { get; }
        public DayConsumptionSource Grid { get; set; }
        public DayConsumptionSource Solar { get; set; }
        public DayConsumptionSource Battery { get; set; }
        public DayConsumptionSource Consumption { get; }

        public HomeConsumptionDaily(
            DateTime date,
            DayConsumptionSource consumption,
            DayConsumptionSource grid,
            DayConsumptionSource solarConsumption,
            DayConsumptionSource batteryConsumption,
            DayConsumptionSource batteryCharge,
            DayConsumptionSource solarExported)
        {
            Battery = batteryConsumption;
            Consumption = consumption;
            Solar = solarConsumption;
            Grid = grid;
            Date = date;
            BatteryCharge = batteryCharge;
            SolarExported = solarExported;
            // missing battery exported
        }
    }

    public class DayConsumptionSource
    {
        public IEnumerable<(TimeSpan TimeOfDay, decimal? WattHours)> UsageSummary { get; set; }

        public DayConsumptionSource(IEnumerable<(TimeSpan TimeOfDay, decimal? Stats)> summary)
        {
            UsageSummary = summary;
        }
    }
}
