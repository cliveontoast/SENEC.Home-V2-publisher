using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class VoltageMomentDaily
    {
        public DateTime Date { get; set; }
        public DayVoltageMoments L1 { get; set; }
        public DayVoltageMoments L2 { get; set; }
        public DayVoltageMoments L3 { get; set; }

        public VoltageMomentDaily(DayVoltageMoments l3, DayVoltageMoments l2, DayVoltageMoments l1, DateTime date)
        {
            L3 = l3;
            L2 = l2;
            L1 = l1;
            Date = date;
        }
    }

    public class DayVoltageMoments
    {
        public IEnumerable<(TimeSpan TimeOfDay, decimal? value)> PhaseMoments { get; set; }

        public DayVoltageMoments(IEnumerable<(TimeSpan TimeOfDay, decimal? value)> phaseMoments)
        {
            PhaseMoments = phaseMoments;
        }
    }
}
