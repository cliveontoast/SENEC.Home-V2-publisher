using MediatR;
using System;

namespace Entities
{
    public class VoltageSummary : INotification, IRepositoryEntity
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public StatisticV1 L1 { get; set; }
        public StatisticV1 L2 { get; set; }
        public StatisticV1 L3 { get; set; }

        public VoltageSummary(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, StatisticV1 l3, StatisticV1 l2, StatisticV1 l1)
        {
            IntervalEndExcluded = intervalEndExcluded;
            IntervalStartIncluded = intervalStartIncluded;
            L3 = l3;
            L2 = l2;
            L1 = l1;
        }
    }
}


