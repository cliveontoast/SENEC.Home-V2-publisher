using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class PowerMovementDaily
    {
        public DateTime Date { get; set; }
        public DayConsumptionSource SolarToHome { get; }
        public DayConsumptionSource SolarToBattery { get; }
        public DayConsumptionSource SolarToGrid { get; }
        public DayConsumptionSource GridToBattery { get; set; }
        public DayConsumptionSource GridToHome { get; set; }
        public DayConsumptionSource BatteryToHome { get; set; }
        public DayConsumptionSource BatteryToGrid { get; set; }
        public DayConsumptionSource Consumption { get; }

        public PowerMovementDaily(DateTime date,
            DayConsumptionSource consumption,
            DayConsumptionSource solarToHome,
            DayConsumptionSource solarToBattery,
            DayConsumptionSource solarToGrid,
            DayConsumptionSource gridToBattery,
            DayConsumptionSource gridToHome,
            DayConsumptionSource batteryToHome,
            DayConsumptionSource batteryToGrid)
        {
            Date = date;
            Consumption = consumption;
            SolarToHome = solarToHome;
            SolarToBattery = solarToBattery;
            SolarToGrid = solarToGrid;
            GridToBattery = gridToBattery;
            GridToHome = gridToHome;
            BatteryToHome = batteryToHome;
            BatteryToGrid = batteryToGrid;
        }
    }
}
