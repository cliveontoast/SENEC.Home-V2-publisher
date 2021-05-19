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
        public decimal? MaximumBatteryCelsius { get; private set; }
        public decimal? MaximumCaseCelsius { get; private set; }
        public int SecondsWithoutData { get; set; }

        public BatteryInverterTemperatureSummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, int version,
            decimal? batteryCelsius,
            decimal? caseCelsius,
            List<decimal?> maximumTemperature,
            int secondsWithoutData)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Version = version;

            MaximumTemperature = maximumTemperature;
            MaximumBatteryCelsius = batteryCelsius;
            MaximumCaseCelsius = caseCelsius;
            SecondsWithoutData = secondsWithoutData;
        }
    }
}
