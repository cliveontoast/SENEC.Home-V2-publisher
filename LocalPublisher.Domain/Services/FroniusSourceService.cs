using System;

namespace Domain
{
    public class FroniusSourceService : ITimedService
    {
        public Type Command { get => typeof(FroniusPollCommand); }
        public TimeSpan Period => TimeSpan.FromMilliseconds(1000);
    }
}
