using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class InverterTemperatures
    {
        public DateTimeOffset Instant { get; set; }
        public List<SenecDecimal> Temperatures { get; set; }

        public InverterTemperatures(
            DateTimeOffset instant,
            List<SenecDecimal> temperatures)
        {
            Instant = instant;
            Temperatures = temperatures;
        }

        public MomentBatteryInverterTemperatures GetMoment()
        {
            return new MomentBatteryInverterTemperatures(Instant,
                Temperatures.All(a => a.HasValue),
                Temperatures.Select(a => a.Value));
        }
    }
}
