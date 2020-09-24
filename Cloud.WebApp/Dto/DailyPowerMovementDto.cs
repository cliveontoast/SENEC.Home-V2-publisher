using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuanceWebApp.Dto
{
    public class DailyPowerMovementDto
    {
        public DailyPowerMovementDto(PowerMovementDaily result)
        {
            Date = new DateTimeOffset(result.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
            ToHome = new HomeConsumptionDto("Home consumption", result.Consumption.UsageSummary.Select(a => a.WattHours));
            FromGridToHome = new HomeConsumptionDto("From grid to home", result.GridToHome.UsageSummary.Select(a => a.WattHours));
            FromGridToBattery = new HomeConsumptionDto("From grid to battery", result.GridToBattery.UsageSummary.Select(a => a.WattHours));
            FromBatteryToHome = new HomeConsumptionDto("From battery to home", result.BatteryToHome.UsageSummary.Select(a => a.WattHours));
            FromBatteryToHomeNeg = new HomeConsumptionDto("From battery to home", result.BatteryToHome.UsageSummary.Select(a => -a.WattHours));
            FromBatteryToCommunity = new HomeConsumptionDto("From battery to community", result.BatteryToGrid.UsageSummary.Select(a => a.WattHours));
            FromBatteryToCommunityNeg = new HomeConsumptionDto("From battery to community", result.BatteryToGrid.UsageSummary.Select(a => -a.WattHours));
            FromSunToBattery = new HomeConsumptionDto("From sun to battery", result.SolarToBattery.UsageSummary.Select(a => a.WattHours));
            FromSunToGrid = new HomeConsumptionDto("From sun to grid", result.SolarToGrid.UsageSummary.Select(a => a.WattHours));
            FromSunToHome = new HomeConsumptionDto("From sun to home", result.SolarToHome.UsageSummary.Select(a => a.WattHours));
        }

        public long Date { get; private set; }
        public HomeConsumptionDto ToHome { get; }
        public HomeConsumptionDto FromGridToHome { get; }
        public HomeConsumptionDto FromGridToBattery { get; }
        public HomeConsumptionDto FromBatteryToHome { get; }
        public HomeConsumptionDto FromBatteryToHomeNeg { get; }
        public HomeConsumptionDto FromBatteryToCommunity { get; }
        public HomeConsumptionDto FromBatteryToCommunityNeg { get; }
        public HomeConsumptionDto FromSunToBattery { get; }
        public HomeConsumptionDto FromSunToGrid { get; }
        public HomeConsumptionDto FromSunToHome { get; }
        public IEnumerable<string> XLabels { get; }
    }
}
