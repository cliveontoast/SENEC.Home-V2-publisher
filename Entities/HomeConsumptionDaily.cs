using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class HomeConsumptionDaily
    {
        public DateTime Date { get; set; }
        public DayConsumptionSource BatteryEstimate { get; }

        public DayConsumptionSource Grid { get; set; }
        public DayConsumptionSource Solar { get; set; }
        public DayConsumptionSource Battery { get; set; }
        public DayConsumptionSource Consumption { get; }

        public HomeConsumptionDaily(DateTime date, DayConsumptionSource consumption, DayConsumptionSource grid, DayConsumptionSource solar, DayConsumptionSource battery)
        {
            Battery = battery;
            Consumption = consumption;
            Solar = solar;
            Grid = grid;
            Date = date;
            BatteryEstimate = GetEstimate();
        }

        private DayConsumptionSource GetEstimate()
        {
            var batteryValues = new List<(TimeSpan, decimal?)>();
            for (int i = 0; i < Consumption.UsageSummary.Count(); i++)
            {
                var consumptionItem = Consumption.UsageSummary.Skip(i).First();
                var consumptionValue = consumptionItem.WattHours;
                var solar = Solar.UsageSummary.Skip(i).First().WattHours;
                var grid = Grid.UsageSummary.Skip(i).First().WattHours;
                // not a great way. but the only way given the summary data as it is currently stored
                var batteryEstimate = consumptionValue.GetValueOrDefault() - grid.GetValueOrDefault() - solar.GetValueOrDefault();

                batteryValues.Add((consumptionItem.TimeOfDay, consumptionValue.HasValue ? batteryEstimate : (decimal?)null));
            }
            return new DayConsumptionSource(batteryValues);
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
