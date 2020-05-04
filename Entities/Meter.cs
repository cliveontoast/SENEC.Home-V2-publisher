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
    }
}


