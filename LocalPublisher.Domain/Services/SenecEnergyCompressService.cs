using System;

namespace Domain
{
    public class SenecEnergyCompressService : ITimedService
    {
        public Type Command { get => typeof(SenecEnergySummaryCommand); }
        public TimeSpan Period => TimeSpan.FromSeconds(10);
    }
}
