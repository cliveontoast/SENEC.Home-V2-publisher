using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApp.Dto
{
    public class DailyEnergySummaryDto
    {
        public DailyEnergySummaryDto(EnergySummaryDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            BatteryCapacity = new BatteryCapacity("Capacity", result.BatteryCapacity);
            XLabels = result.BatteryCapacity.Percentages.Select(a => a.TimeOfDay.ToString(@"hh\:mm")).ToArray();
        }

        public long Date { get; private set; }
        public BatteryCapacity BatteryCapacity { get; }
        public IEnumerable<string> XLabels { get; }
    }

    public class BatteryCapacity
    {
        public string Label { get; set; }
        public IEnumerable<decimal?> Data { get; set; }

        public BatteryCapacity(string v, DayBatteryPercentage l1)
        {
            Label = v;
            Data = l1.Percentages.Select(a => a.Stats?.Median).ToList();
        }
    }
}
