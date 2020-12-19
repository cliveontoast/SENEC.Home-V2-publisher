using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApp.Dto
{
    public class DailyEquipmentStatesSummaryDto
    {
        public DailyEquipmentStatesSummaryDto(EquipmentStatesSummaryDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            States = result.Summaries.Select(a => new EquipmentState(a.Name, a.Count));
        }

        public long Date { get; private set; }
        public IEnumerable<EquipmentState> States { get; }

        public class EquipmentState
        {
            public string Name { get; set; }
            public IEnumerable<int?> Data { get; set; }

            public EquipmentState(string v, IEnumerable<int?> data)
            {
                Name = v;
                Data = data.ToList();
            }
        }
    }
}
