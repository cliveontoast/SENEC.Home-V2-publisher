using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class MomentBatteryInverterTemperatures : IIsValid
    {
        public List<decimal?> Temperatures { get; }
        public DateTimeOffset Instant { get; }
        public decimal? BatteryCelsius { get; }
        public decimal? CaseCelsius { get; }
        public bool IsValid { get; }

        public MomentBatteryInverterTemperatures(
            DateTimeOffset instant,
            decimal? batteryCelsius,
            decimal? caseCelsius,
            bool isValid, IEnumerable<decimal?> temperatures)
        {
            Instant = instant;
            BatteryCelsius = batteryCelsius;
            CaseCelsius = caseCelsius;
            Temperatures = temperatures.ToList();
            IsValid = isValid;
        }
    }
}
