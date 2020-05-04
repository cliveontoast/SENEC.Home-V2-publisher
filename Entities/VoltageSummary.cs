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
    }
}


