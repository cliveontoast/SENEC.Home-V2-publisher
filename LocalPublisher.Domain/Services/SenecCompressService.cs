using System;

namespace Domain
{
    public class SenecCompressService : ITimedService
    {
        public Type Command { get => typeof(SenecGridMeterSummaryCommand); }
        public TimeSpan Period => TimeSpan.FromSeconds(10);
    }
}
