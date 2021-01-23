using Entities;
using System;
using System.Collections.Generic;

namespace ReadRepository.ReadModel
{
    public class VoltageMomentReadModel : IRepositoryReadModel
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }
        public IEnumerable<MomentVoltage> Moments { get; set; }

        public VoltageMomentReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, IEnumerable<MomentVoltage> moments, int version)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Moments = moments;
            Version = version;
        }
    }
}
