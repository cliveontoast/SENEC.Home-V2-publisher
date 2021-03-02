using System;

namespace Domain
{
    public class TeslaSourceService : ITimedService
    {
        public Type Command { get => typeof(TeslaPowerwall2PollCommand); }
        public TimeSpan Period => TimeSpan.FromSeconds(1);
    }
}
