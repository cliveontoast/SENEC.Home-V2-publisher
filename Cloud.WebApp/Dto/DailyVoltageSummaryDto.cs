using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApp.Dto
{
    public class DailyVoltageSummaryDto
    {
        public DailyVoltageSummaryDto(VoltageSummaryDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            Phases = new[]
            {
                new Phase("L1", result.L1),
                new Phase("L2", result.L2),
                new Phase("L3", result.L3),
            };
            XLabels = result.L1.PhaseSummary.Select(a => a.TimeOfDay.ToString(@"hh\:mm")).ToArray();
        }

        public long Date { get; private set; }
        public IEnumerable<Phase> Phases { get; }
        public IEnumerable<string> XLabels { get; }

        public class Phase
        {
            public string Label { get; set; }
            public IEnumerable<decimal?> Data { get; set; }

            public Phase(string v, DayVoltageSummary l1)
            {
                Label = v;
                Data = l1.PhaseSummary.Select(a => a.Stats?.Median).ToList();
            }
        }
    }
}
