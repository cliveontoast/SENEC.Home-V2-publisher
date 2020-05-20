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
    }

    public class DayVoltageSummary
    {
        public IEnumerable<(TimeSpan TimeOfDay, Statistic? Stats)> PhaseSummary { get; set; }
    }
}
