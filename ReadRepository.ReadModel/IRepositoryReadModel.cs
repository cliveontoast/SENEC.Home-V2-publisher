using System;
using System.Collections.Generic;
using System.Text;

namespace ReadRepository.ReadModel
{
    public interface IRepositoryReadModel
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }
    }
}
