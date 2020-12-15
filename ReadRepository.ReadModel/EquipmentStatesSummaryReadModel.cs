using Entities;
using System;
using System.Collections.Generic;

namespace ReadRepository.ReadModel
{
    public class EquipmentStatesSummaryReadModel : IRepositoryReadModel
    {
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }

        public List<EquipmentStateStatistic> States { get; private set; }

        public int SecondsWithoutData { get; set; }

        public EquipmentStatesSummaryReadModel(DateTimeOffset intervalEndExcluded, DateTimeOffset intervalStartIncluded, string key, int version,
            List<EquipmentStateStatistic> states,
            int secondsWithoutData)
        {
            Key = key;
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Version = version;

            States = states;
            
            SecondsWithoutData = secondsWithoutData;
        }
    }
}
