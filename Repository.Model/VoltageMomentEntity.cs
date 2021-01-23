using Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Model
{
    public class VoltageMomentEntity : IIsValid
    {
        public VoltageMomentEntity() { }
        public decimal? L1 { get; set; }
        public decimal? L2 { get; set; }
        public decimal? L3 { get; set; }
        public DateTimeOffset Instant { get; set; }

        public bool IsValid => L1 != null && L2 != null && L3 != null;

        public VoltageMomentEntity(MomentVoltage? momentVoltage)
        {
            if (momentVoltage?.IsValid == true)
            {
                L1 = momentVoltage.L1;
                L2 = momentVoltage.L2;
                L3 = momentVoltage.L3;
                Instant = momentVoltage.Instant;
            }
        }

        public static implicit operator MomentVoltage(VoltageMomentEntity d)
        {
            return d.IsValid
                ? new MomentVoltage(d.Instant, d.L1!.Value, d.L2!.Value, d.L3!.Value)
                : new MomentVoltage(d.Instant, d.IsValid);
        }
    }
}
