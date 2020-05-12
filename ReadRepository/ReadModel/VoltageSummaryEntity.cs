using Entities;
using Newtonsoft.Json;
using System;

namespace ReadRepository.ReadModel
{
    public class VoltageSummaryReadModel
    {
        public Statistic L1 { get; set; }
        public Statistic L2 { get; set; }
        public Statistic L3 { get; set; }
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; internal set; }
    }
}
