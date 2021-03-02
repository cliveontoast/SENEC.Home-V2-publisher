using System;

namespace Domain
{
    public class TeslaEnergyCompressService : ITimedService
    {
        public Type Command { get => typeof(TeslaEnergySummaryCommand); }
        public TimeSpan Period => TimeSpan.FromSeconds(10);
    }
}
