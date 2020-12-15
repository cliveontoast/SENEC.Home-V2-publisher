using MediatR;
using System;
using System.Collections.Generic;

namespace Entities
{
    public class EquipmentStatesSummary : INotification, IIntervalEntity
    {
        public EquipmentStatesSummary(DateTimeOffset intervalStartIncluded, DateTimeOffset intervalEndExcluded, IEnumerable<EquipmentStateStatistic> states, int secondsWithoutData)
        {
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            States = states;
            SecondsWithoutData = secondsWithoutData;
        }

        public IEnumerable<EquipmentStateStatistic> States { get; set; }
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }
        public int SecondsWithoutData { get; set; }
    }

    public struct EquipmentStateStatistic
    {
        public string StateText { get; set; }
        public int Count { get; set; }

        public EquipmentStateStatistic(string stateText, int count) : this()
        {
            StateText = stateText;
            Count = count;
        }
    }
}
