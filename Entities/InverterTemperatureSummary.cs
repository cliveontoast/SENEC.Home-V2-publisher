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
        public int SecondsWithoutData { get; set; }

        public InverterTemperatureSummary(DateTimeOffset intervalStartIncluded, DateTimeOffset intervalEndExcluded, int secondsWithoutData, params decimal?[] temperatures)
        {
            IntervalEndExcluded = intervalEndExcluded;
            IntervalStartIncluded = intervalStartIncluded;
            SecondsWithoutData = secondsWithoutData;
            Temperatures = temperatures.ToList();
        }
        public InverterTemperatureSummary(DateTimeOffset intervalStartIncluded, DateTimeOffset intervalEndExcluded, int secondsWithoutData, params Statistic[] temperatures)
            :this (intervalStartIncluded, intervalEndExcluded, secondsWithoutData, temperatures.Select(a => a.IsValid ? a.Maximum : (decimal?)null).ToArray())
        {

        }
    }
}


