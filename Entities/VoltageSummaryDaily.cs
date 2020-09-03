using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class VoltageSummaryDaily
    {
        public DateTime Date { get; set; }
        public DayVoltageSummary L1 { get; set; }
        public DayVoltageSummary L2 { get; set; }
        public DayVoltageSummary L3 { get; set; }

        public VoltageSummaryDaily(DayVoltageSummary l3, DayVoltageSummary l2, DayVoltageSummary l1, DateTime date)
        {
            L3 = l3;
            L2 = l2;
            L1 = l1;
            Date = date;
        }
    }

    public class DayVoltageSummary
    {
        public IEnumerable<(TimeSpan TimeOfDay, Statistic? Stats)> PhaseSummary { get; set; }

        public DayVoltageSummary(IEnumerable<(TimeSpan TimeOfDay, Statistic? Stats)> phaseSummary)
        {
            PhaseSummary = phaseSummary;
        }
    }
}
