using System;

namespace Shared
{
    public interface ITimeProvider
    {
        DateTimeOffset Now { get; }
    }
}
