using System;

namespace Domain
{
    public interface ITimedService
    {
        Type Command { get; }
        TimeSpan Period { get; }
    }
}
