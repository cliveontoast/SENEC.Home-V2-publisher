using System;

namespace Shared
{
    public class TimeProvider : ITimeProvider
    {
        public DateTimeOffset Now { get => DateTimeOffset.Now; }
    }
}
