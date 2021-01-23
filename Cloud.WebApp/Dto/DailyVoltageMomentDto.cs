using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApp.Dto
{
    public class DailyVoltageMomentDto
    {
        public DailyVoltageMomentDto(VoltageMomentDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            Phases = new[]
            {
                new Phase("L1", result.L1),
                new Phase("L2", result.L2),
                new Phase("L3", result.L3),
            };
        }

        public long Date { get; private set; }
        public IEnumerable<Phase> Phases { get; }

        public class Phase
        {
            public string Label { get; set; }
            public IEnumerable<decimal?> Data { get; set; }

            public Phase(string v, DayVoltageMoments l1)
            {
                Label = v;
                Data = l1.PhaseMoments.Select(a => a.value).ToList();
            }
        }
    }
}
