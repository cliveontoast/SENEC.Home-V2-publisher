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
            ToBattery = new HomeConsumptionDto("To battery", result.BatteryCharge.UsageSummary.Select(a => a.WattHours));
            ToCommunity = new HomeConsumptionDto("Solar to community", result.SolarExported.UsageSummary.Select(a => a.WattHours));
        }

        public DailyHomeConsumptionDto(HomeConsumptionDaily result, PowerMovementDaily powerFlowResult)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            ToHome = new HomeConsumptionDto("Home consumption", result.Consumption.UsageSummary.Select(a => a.WattHours));
            FromGridToBattery = new HomeConsumptionDto("Grid to Battery", powerFlowResult.GridToBattery.UsageSummary.Select(a => a.WattHours));
            FromGrid = new HomeConsumptionDto("Grid to Home", powerFlowResult.GridToHome.UsageSummary.Select(a => a.WattHours == null ? (decimal?)null : Math.Max(0, a.WattHours.Value)));
            FromSolar = new HomeConsumptionDto("From sun", result.Solar.UsageSummary.Select(a => a.WattHours));
            FromBattery = new HomeConsumptionDto("From battery", powerFlowResult.BatteryToHome.UsageSummary.Select(a => a.WattHours));
            ToBattery = new HomeConsumptionDto("To battery", powerFlowResult.SolarToBattery.UsageSummary.Select(a => a.WattHours));
            ToCommunity = new HomeConsumptionDto("Solar to community", result.SolarExported.UsageSummary.Select(a => a.WattHours));
        }

        public long Date { get; private set; }
        public HomeConsumptionDto ToHome { get; }
        public HomeConsumptionDto FromGrid { get; } // to home
        public HomeConsumptionDto FromGridToBattery { get; }
        public HomeConsumptionDto FromSolar { get; }
        public HomeConsumptionDto FromBattery { get; }
        public HomeConsumptionDto ToBattery { get; } // solar to battery
        public HomeConsumptionDto ToCommunity { get; }
        public IEnumerable<string> XLabels { get; }
        public MoneyPlan[] MoneyPlans { get; internal set; }
    }

    public class MoneyPlan
    {
        public string Name { get; }
        public decimal Cost { get; }

        public MoneyPlan(string name, decimal cost)
        {
            Name = name;
            Cost = cost;
        }
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
