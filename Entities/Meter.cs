using System;

namespace Entities
{
    public class Meter
    {
        public SenecDecimal Frequency { get; set; }
        public MeterPhase L1 { get; set; }
        public MeterPhase L2 { get; set; }
        public MeterPhase L3 { get; set; }
        public SenecDecimal TotalPower { get; set; }
        public DateTimeOffset Instant { get; set; }

        public Meter(DateTimeOffset instant, SenecDecimal totalPower, MeterPhase l3, MeterPhase l2, MeterPhase l1, SenecDecimal frequency)
        {
            Instant = instant;
            TotalPower = totalPower;
            L3 = l3;
            L2 = l2;
            L1 = l1;
            Frequency = frequency;
        }
    }
}


