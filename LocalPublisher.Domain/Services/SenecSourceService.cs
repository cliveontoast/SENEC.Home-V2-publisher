using System;

namespace Domain
{
    public class SenecSourceService : ITimedService
    {
        public Type Command { get => typeof(SenecPollCommand); }
        public TimeSpan Period => TimeSpan.FromSeconds(1);
    }
}
