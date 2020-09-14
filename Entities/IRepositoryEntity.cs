using System;

namespace Entities
{
    public interface IRepositoryEntity
    {
        DateTimeOffset IntervalStartIncluded { get; set; }
        DateTimeOffset IntervalEndExcluded { get; set; }
    }
}