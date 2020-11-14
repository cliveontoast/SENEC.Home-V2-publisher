using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class TemperatureInverterSummaryDaily
    {
        public DateTime Date { get; set; }
        public DayTemperatureSummary T1 { get; set; }
        public DayTemperatureSummary T2 { get; set; }
        public DayTemperatureSummary T3 { get; set; }
        public DayTemperatureSummary T4 { get; set; }
        public DayTemperatureSummary T5 { get; set; }

        public TemperatureInverterSummaryDaily(
            DayTemperatureSummary t1,
            DayTemperatureSummary t2,
            DayTemperatureSummary t3,
            DayTemperatureSummary t4,
            DayTemperatureSummary t5,
            DateTime date)
        {
            T5 = t5;
            T4 = t4;
            T3 = t3;
            T2 = t2;
            T1 = t1;
            Date = date;
        }
    }

    public class DayTemperatureSummary
    {
        public IEnumerable<(TimeSpan TimeOfDay, decimal? Stats)> Summary { get; set; }

        public DayTemperatureSummary(IEnumerable<(TimeSpan TimeOfDay, decimal? Stats)> phaseSummary)
        {
            Summary = phaseSummary;
        }
    }
}
