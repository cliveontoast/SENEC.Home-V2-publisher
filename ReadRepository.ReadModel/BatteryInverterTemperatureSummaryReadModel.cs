using Entities;
using System;
using System.Collections.Generic;

namespace ReadRepository.ReadModel
{
    public class BatteryInverterTemperatureSummaryReadModel : IRepositoryReadModel
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }

        public List<decimal?> MaximumTemperature { get; set; }

        public int SecondsWithoutData { get; set; }

        public BatteryInverterTemperatureSummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, int version,
            List<decimal?> maximumTemperature,
            int secondsWithoutData)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Version = version;

            MaximumTemperature = maximumTemperature;
            
            SecondsWithoutData = secondsWithoutData;
        }
    }
}
