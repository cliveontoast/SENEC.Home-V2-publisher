using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApp.Dto
{
    public class DailyTemperatureSummaryDto
    {
        public DailyTemperatureSummaryDto(TemperatureInverterSummaryDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            Temps = new[]
            {
                new Temperatures("T1", result.T1),
                new Temperatures("T2", result.T2),
                new Temperatures("T3", result.T3),
                new Temperatures("T4", result.T4),
                new Temperatures("T5", result.T5),
            };
        }

        public long Date { get; private set; }
        public IEnumerable<Temperatures> Temps { get; }

        public class Temperatures
        {
            public string Label { get; set; }
            public IEnumerable<decimal?> Data { get; set; }

            public Temperatures(string v, DayTemperatureSummary l1)
            {
                Label = v;
                Data = l1.Summary.Select(a => a.Stats).ToList();
            }
        }
    }
}
