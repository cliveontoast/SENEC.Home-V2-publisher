using System;

namespace Domain
{
    public class SenecBatteryInverterCompressService : ITimedService
    {
        public Type Command { get => typeof(SenecBatteryInverterSummaryCommand); }
        public TimeSpan Period => TimeSpan.FromSeconds(10);
    }
}
