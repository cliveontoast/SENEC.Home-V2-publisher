using MediatR;
using System;
using System.Collections.Generic;

namespace Entities
{
    public class EnergySummaryDaily : INotification
    {
        public DateTime Date { get; set; }
        public DayBatteryPercentage BatteryCapacity { get; set; }

        public EnergySummaryDaily(DateTime date, DayBatteryPercentage batteryCapacity)
        {
            BatteryCapacity = batteryCapacity;
            Date = date;
        }
    }

    public class DayBatteryPercentage
    {
        public IEnumerable<(TimeSpan TimeOfDay, Statistic? Stats)> Percentages { get; set; }

        public DayBatteryPercentage(IEnumerable<(TimeSpan TimeOfDay, Statistic? Stats)> summary)
        {
            Percentages = summary;
        }
    }
}
