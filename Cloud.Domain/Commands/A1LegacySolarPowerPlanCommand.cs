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
        private const decimal GridWattHour2022 = 30.0605m;
        private const decimal GridToBattery2022 = -BatteryToGrid2022;
        private const decimal SolarToGrid2022 = -7.135m;
        private const decimal SolarToHome2022 = -GridWattHour2022-SolarToGrid2022; // -23 cents
        private const decimal SolarToBattery2022 = SolarToGrid2022;
        private const decimal BatteryToHome2022 = -GridWattHour2022;
        private const decimal BatteryToGrid2022 = SolarToGrid2022;
        private const decimal SupplyCharge2022 = 107.7685m;

        public A1LegacySolarPowerPlanCommandHandler()
        {
        }

        public async Task<PowerPlanReadModel> Handle(A1LegacySolarPowerPlanCommand request, CancellationToken cancellationToken)
        {
            var batteryToHome = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.BatteryToHome ?? a.home.Battery,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = BatteryToHome2022,
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
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = BatteryToGrid2022,
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
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = -BatteryToGrid2022,
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
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = GridWattHour2022,
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
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = SolarToBattery2022,
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
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = SolarToGrid2022,
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
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = SolarToHome2022,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 0m,
                }
            });

            await Task.FromResult(0);
            return new PowerPlanReadModel
            {
                Dollars = Math.Round(
                    batteryToGrid
                    .Union(batteryToHome)
                    .Union(gridToBattery)
                    .Union(gridToHome)
                    .Union(solarToBattery)
                    .Union(solarToGrid)
                    .Union(solarToHome)
                    .Sum() / 100, 2)
            };
        }

    }
}
