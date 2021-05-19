using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class InverterTemperatures
    {
        public DateTimeOffset Instant { get; set; }
        public SenecDecimal BatteryCelsius { get; set; }
        public SenecDecimal CaseCelsius { get; set; }
        public List<SenecDecimal> Temperatures { get; set; }

        public InverterTemperatures(
            DateTimeOffset instant,
            SenecDecimal batteryCelsius,
            SenecDecimal caseCelsius,
            List<SenecDecimal> temperatures)
        {
            Instant = instant;
            BatteryCelsius = batteryCelsius;
            CaseCelsius = caseCelsius;
            Temperatures = temperatures;
        }

        public MomentBatteryInverterTemperatures GetMoment()
        {
            return new MomentBatteryInverterTemperatures(Instant,
                BatteryCelsius.Value,
                CaseCelsius.Value,
                Temperatures.All(a => a.HasValue),
                Temperatures.Select(a => a.Value));
        }
    }
}
