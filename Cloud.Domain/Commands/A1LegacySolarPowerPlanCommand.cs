using Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class A1LegacySolarPowerPlanCommand : IRequest<PowerPlanReadModel>
    {
        public HomeConsumptionDaily HomeConsumption { get; set; }
        public PowerMovementDaily? PowerMovements { get; set; }

        public A1LegacySolarPowerPlanCommand(HomeConsumptionDaily homeConsumption, PowerMovementDaily? powerMovements)
        {
            HomeConsumption = homeConsumption;
            PowerMovements = powerMovements;
        }
    }

    public class A1LegacySolarPowerPlanCommandHandler : IRequestHandler<A1LegacySolarPowerPlanCommand, PowerPlanReadModel>
    {
        private decimal GridWattHour2022 => 30.0605m;
        private decimal GridToBattery2022 => -BatteryToGrid2022;
        private decimal SolarToGrid2022 => -7.135m;
        private decimal SolarToHome2022 => 0; //instead of costing 30c to import power - this costs 0, but is 'lost earnings' of 7c
        private decimal SolarToBattery2022 => SolarToGrid2022;
        private decimal BatteryToHome2022 => GridWattHour2022;
        private decimal BatteryToGrid2022 => 0;
        private decimal SupplyCharge2022 => 107.7685m;

        public async Task<PowerPlanReadModel> Handle(A1LegacySolarPowerPlanCommand request, CancellationToken cancellationToken)
        {
            var batteryToHome = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.BatteryToHome ?? a.home.Battery,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = BatteryToHome2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });
            var batteryToGrid = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.BatteryToGrid ?? new DayConsumptionSource(Enumerable.Empty<(TimeSpan, decimal?)>()),
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = BatteryToGrid2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });

            var gridToBattery = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.GridToBattery ?? new DayConsumptionSource(Enumerable.Empty<(TimeSpan, decimal?)>()),
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = GridToBattery2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });

            var gridToHome = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.GridToHome ?? a.home.Grid,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = GridWattHour2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = SupplyCharge2022,
                }
            });

            var solarToBattery = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.SolarToBattery ?? a.home.BatteryCharge,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = SolarToBattery2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });

            var solarToGrid = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.SolarToGrid ?? a.home.SolarExported,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = SolarToGrid2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });

            var solarToHome = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.SolarToHome ?? a.home.Solar,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0, 0, 0),
                        To = new TimeSpan(24, 0, 0),
                        KiloWattRate = SolarToHome2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });

            await Task.FromResult(0);
            var cents = batteryToGrid.Sum()
                + batteryToHome.Sum()
                + gridToBattery.Sum()
                + gridToHome.Sum()
                + solarToBattery.Sum()
                + solarToGrid.Sum()
                + solarToHome.Sum();
            return new PowerPlanReadModel
            {
                Dollars = Math.Round(cents / 100, 2)
            };
        }
    }
}
