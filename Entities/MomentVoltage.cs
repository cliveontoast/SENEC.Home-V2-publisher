using System;

namespace Entities
{
    public class MomentVoltage
    {
        public decimal L1 { get; }
        public decimal L2 { get; }
        public decimal L3 { get; }
        public DateTimeOffset Instant { get; }
        public bool IsValid { get; }

        public MomentVoltage(DateTimeOffset instant, decimal l1, decimal l2, decimal l3)
            : this(instant, true)
        {
            L1 = l1;
            L2 = l2;
            L3 = l3;
        }

        public MomentVoltage(DateTimeOffset instant, bool isValid)
        {
            Instant = instant;
            IsValid = isValid;
        }
    }
}
