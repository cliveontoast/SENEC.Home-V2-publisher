using Entities;
using System;

namespace ReadRepository.ReadModel
{
    public class VoltageSummaryReadModel : IRepositoryReadModel
    {
        public StatisticV1 L1 { get; set; }
        public StatisticV1 L2 { get; set; }
        public StatisticV1 L3 { get; set; }
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }

        public VoltageSummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, StatisticV1 l1, StatisticV1 l2, StatisticV1 l3, int version)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            L1 = l1;
            L2 = l2;
            L3 = l3;
            Version = version;
        }
    }
}
