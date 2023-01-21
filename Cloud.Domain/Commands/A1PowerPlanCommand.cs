using Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class A1PowerPlanCommand : IRequest<PowerPlanReadModel>
    {
        public HomeConsumptionDaily HomeConsumption { get; set; }
        public PowerMovementDaily? PowerMovements { get; set; }

        public A1PowerPlanCommand(HomeConsumptionDaily homeConsumption, PowerMovementDaily? powerMovements)
        {
            HomeConsumption = homeConsumption;
            PowerMovements = powerMovements;
        }
    }

    public class A1PowerPlanCommandHandler : IRequestHandler<A1PowerPlanCommand, PowerPlanReadModel>
    {
        public A1PowerPlanCommandHandler()
        {
        }

        public async Task<PowerPlanReadModel> Handle(A1PowerPlanCommand request, CancellationToken cancellationToken)
        {
            var costModel = new[]
            {
                new PowerModel(
                    a => a.home.Consumption,
                    new ElectricalCharge
                    {
                        From = new TimeSpan(0),
                        To = new TimeSpan(24,0,0),
                        Rate = 30.0605m,
                    })
                {
                    From = new DateTime(2022, 7, 1),
                    To = new DateTime(3000,1,1),
                    SupplyChargePerDay = 107.7685m,
                }
            };
            var costs = GetCents(request.HomeConsumption, request.PowerMovements, costModel);
            await Task.FromResult(0);
            return new PowerPlanReadModel
            {
                Dollars = Math.Round(costs.Sum() / 100, 2)
            };
        }


        internal static IEnumerable<decimal> GetCents(HomeConsumptionDaily home, PowerMovementDaily? power, PowerModel[] models)
        {
            return from model in models
                   where model.From <= home.Date
                   where model.To > home.Date
                   from usage in model.Selector((home, power)).UsageSummary
                   from slot in model.ElectricalCharges
                   where slot.From <= usage.TimeOfDay
                   where slot.To > usage.TimeOfDay
                   select (model.SupplyChargePer1Minute + slot.Rate * Math.Max(0, usage.WattHours ?? 0) / 60) * 5;
        }
    }

    internal class ElectricalCharge
    {
        public decimal Rate { get; internal set; }
        public TimeSpan To { get; internal set; }
        public TimeSpan From { get; internal set; }
    }

    internal class PowerModel
    {
        public DateTime From { get; internal set; }
        public DateTime To { get; internal set; }
        public decimal SupplyChargePerDay { get; internal set; }
        public decimal SupplyChargePer1Minute => SupplyChargePerDay / 24 / 60;
        public ElectricalCharge[] ElectricalCharges { get; internal set; }
        public Func<(HomeConsumptionDaily home, PowerMovementDaily? power), DayConsumptionSource> Selector;

        public PowerModel(
            Func<(HomeConsumptionDaily home, PowerMovementDaily? power), DayConsumptionSource> selector,
            params ElectricalCharge[] electricalCharges)
        {
            ElectricalCharges = electricalCharges;
            Selector = selector;
        }
    }

    public class PowerPlanReadModel
    {
        public decimal Dollars { get; internal set; }
    }
}
