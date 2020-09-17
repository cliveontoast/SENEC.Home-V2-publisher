using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuanceWebApp.Dto
{
    public class DailyHomeConsumptionDto
    {
        public DailyHomeConsumptionDto(HomeConsumptionDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            ToHome = new HomeConsumptionDto("Home consumption", result.Consumption.UsageSummary.Select(a => a.WattHours));
            FromGrid = new HomeConsumptionDto("From grid", result.Grid.UsageSummary.Select(a => a.WattHours));
            FromSolar = new HomeConsumptionDto("From sun", result.Solar.UsageSummary.Select(a => a.WattHours));
            FromBattery = new HomeConsumptionDto("From battery", result.Battery.UsageSummary.Select(a => a.WattHours));
        }

        public long Date { get; private set; }
        public HomeConsumptionDto ToHome { get; }
        public HomeConsumptionDto FromGrid { get; }
        public HomeConsumptionDto FromSolar { get; }
        public HomeConsumptionDto FromBattery { get; }
        public IEnumerable<string> XLabels { get; }
    }

    public class HomeConsumptionDto
    {
        public string Label { get; set; }
        public IEnumerable<decimal?> Data { get; set; }

        public HomeConsumptionDto(string v, IEnumerable<decimal?> l1)
        {
            Label = v;
            Data = l1.ToList();
        }
    }
}
