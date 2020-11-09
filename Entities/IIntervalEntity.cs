using System;

namespace Entities
{
    public interface IIntervalEntity
    {
        DateTimeOffset IntervalStartIncluded { get; set; }
        DateTimeOffset IntervalEndExcluded { get; set; }
    }
}