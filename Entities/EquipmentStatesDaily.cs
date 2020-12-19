using System;
using System.Collections.Generic;

namespace Entities
{
    public class EquipmentStatesSummaryDaily
    {
        public DateTime Date { get; set; }
        public IEnumerable<DayEquipmentStatesSummary> Summaries { get; set; }

        public EquipmentStatesSummaryDaily(
            IEnumerable<DayEquipmentStatesSummary> summaries,
            DateTime date)
        {
            Summaries = summaries;
            Date = date;
        }
    }

    public class DayEquipmentStatesSummary
    {
        public string Name { get; set; }
        public IEnumerable<int?> Count { get; set; }

        public DayEquipmentStatesSummary(string name, IEnumerable<int?> count)
        {
            Name = name;
            Count = count;
        }
    }
}
