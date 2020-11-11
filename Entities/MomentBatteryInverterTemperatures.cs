using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class MomentBatteryInverterTemperatures : IIsValid
    {
        public List<decimal?> Temperatures { get; }
        public DateTimeOffset Instant { get; }
        public bool IsValid { get; }

        public MomentBatteryInverterTemperatures(DateTimeOffset instant, bool isValid, IEnumerable<decimal?> temperatures)
        {
            Instant = instant;
            Temperatures = temperatures.ToList();
            IsValid = isValid;
        }
    }
}
