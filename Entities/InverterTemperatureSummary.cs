using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class InverterTemperatureSummary : INotification, IIntervalEntity
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public List<decimal?> Temperatures { get; set; }
        public decimal? CaseCelsius { get; set; }
        public decimal? BatteryCelsius { get; set; }
        public int SecondsWithoutData { get; set; }

        public InverterTemperatureSummary(
            DateTimeOffset intervalStartIncluded,
            DateTimeOffset intervalEndExcluded,
            int secondsWithoutData,
            decimal? caseCelsius,
            decimal? batteryCelsius,
            params decimal?[] temperatures)
        {
            IntervalEndExcluded = intervalEndExcluded;
            IntervalStartIncluded = intervalStartIncluded;
            SecondsWithoutData = secondsWithoutData;
            Temperatures = temperatures.ToList();
            CaseCelsius = caseCelsius;
            BatteryCelsius = batteryCelsius;
        }
        public InverterTemperatureSummary(
            DateTimeOffset intervalStartIncluded,
            DateTimeOffset intervalEndExcluded,
            int secondsWithoutData,
            Statistic caseCelsius,
            Statistic batteryCelsius,
            params Statistic[] temperatures)
            : this(intervalStartIncluded,
                 intervalEndExcluded,
                 secondsWithoutData,
                 caseCelsius.IsValid ? caseCelsius.Maximum : (decimal?)null,
                 batteryCelsius.IsValid ? batteryCelsius.Maximum : (decimal?)null,
                 temperatures.Select(a => a.IsValid ? a.Maximum : (decimal?)null).ToArray())
        {

        }
    }
}


