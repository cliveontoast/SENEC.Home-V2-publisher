using Entities;
using System;

namespace ReadRepository.ReadModel
{
    public class EnergySummaryReadModel
    {
        //public StatisticV1 L1 { get; set; }
        //public StatisticV1 L2 { get; set; }
        //public StatisticV1 L3 { get; set; }
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }

        public EnergySummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, int version)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Version = version;
        }
    }
}
