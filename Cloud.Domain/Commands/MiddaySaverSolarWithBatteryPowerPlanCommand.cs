using Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class MiddaySaverSolarWithBatteryPowerPlanCommand : IRequest<PowerPlanReadModel>
    {
        public HomeConsumptionDaily HomeConsumption { get; set; }
        public PowerMovementDaily? PowerMovements { get; set; }

        public MiddaySaverSolarWithBatteryPowerPlanCommand(HomeConsumptionDaily homeConsumption, PowerMovementDaily? powerMovements)
        {
            HomeConsumption = homeConsumption;
            PowerMovements = powerMovements;
        }
    }

    public class MiddaySaverSolarWithBatteryPowerPlanCommandHandler : IRequestHandler<MiddaySaverSolarWithBatteryPowerPlanCommand, PowerPlanReadModel>
    {
        private const decimal GridWattHourSuperOffPeak2022 = 8m;
        private const decimal GridWattHourPeak2022 = 50m;
        private const decimal GridWattHourOffPeak2022 = 22m;
        private const decimal SupplyCharge2022 = 120m;
        private const decimal SolarToGrid2022 = -7.135m;
        private const decimal SolarToBattery2022 = -SolarToGrid2022;
        private const decimal BatteryToGrid2022 = SolarToGrid2022;

        private decimal SolarToHome2022(decimal GridWattHour)
        {
            return -GridWattHour - SolarToGrid2022;
        }
        private decimal BatteryToHome2022(decimal GridWattHour)
        {
            return -GridWattHour;
        }

        public MiddaySaverSolarWithBatteryPowerPlanCommandHandler()
        {
        }

        public async Task<PowerPlanReadModel> Handle(MiddaySaverSolarWithBatteryPowerPlanCommand request, CancellationToken cancellationToken)
        {
            var batteryToHome = A1PowerPlanCommandHandler.GetCents(request.HomeConsumption, request.PowerMovements, new[]
            {
                new PowerModel(
                    a => a.power?.BatteryToHome ?? a.home.Battery,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0),
                        To = new TimeSpan(9),
                        Rate = BatteryToHome2022(GridWattHourOffPeak2022),
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(9),
                        To = new TimeSpan(15),
                        Rate = BatteryToHome2022(GridWattHourSuperOffPeak2022),
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(15),
                        To = new TimeSpan(21),
                        Rate = BatteryToHome2022(GridWattHourPeak2022),
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(21),
                        To = new TimeSpan(24),
                        Rate = BatteryToHome2022(GridWattHourOffPeak2022),
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
                        To = new TimeSpan(9),
                        Rate = GridWattHourOffPeak2022,
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(9),
                        To = new TimeSpan(15),
                        Rate = GridWattHourSuperOffPeak2022,
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(15),
                        To = new TimeSpan(21),
                        Rate = GridWattHourPeak2022,
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(21),
                        To = new TimeSpan(24),
                        Rate = GridWattHourOffPeak2022,
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
                        To = new TimeSpan(9),
                        Rate = GridWattHourOffPeak2022,
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(9),
                        To = new TimeSpan(15),
                        Rate = GridWattHourSuperOffPeak2022,
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(15),
                        To = new TimeSpan(21),
                        Rate = GridWattHourPeak2022,
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(21),
                        To = new TimeSpan(24),
                        Rate = GridWattHourOffPeak2022,
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
                        To = new TimeSpan(9),
                        Rate = SolarToHome2022(GridWattHourOffPeak2022),
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(9),
                        To = new TimeSpan(15),
                        Rate = SolarToHome2022(GridWattHourSuperOffPeak2022),
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(15),
                        To = new TimeSpan(21),
                        Rate = SolarToHome2022(GridWattHourPeak2022),
                    },
                    new ElectricalCharge
                    {
                        From = new TimeSpan(21),
                        To = new TimeSpan(24),
                        Rate = SolarToHome2022(GridWattHourOffPeak2022),
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
