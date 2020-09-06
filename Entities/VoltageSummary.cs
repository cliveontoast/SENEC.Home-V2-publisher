using MediatR;
using System;

namespace Entities
{
    public class VoltageSummary : INotification
    {
        public Statistic L1 { get; set; }
        public Statistic L2 { get; set; }
        public Statistic L3 { get; set; }
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }

        public VoltageSummary(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, Statistic l3, Statistic l2, Statistic l1)
        {
            IntervalEndExcluded = intervalEndExcluded;
            IntervalStartIncluded = intervalStartIncluded;
            L3 = l3;
            L2 = l2;
            L1 = l1;
        }
    }
}


