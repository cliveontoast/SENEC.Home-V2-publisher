using System;
using System.Threading.Tasks;
using Domain.Commands;
using Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NuanceWebApp.Dto;
using Serilog;

namespace NuanceWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyHomeConsumptionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public DailyHomeConsumptionController(
            ILogger logger,

            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("today")]
        public IActionResult Today()
        {
            try
            {
                _logger.Debug("Received {Controller} {Name}",
                    nameof(DailyHomeConsumptionController),
                    nameof(Today));
                return Ok(DateTimeOffset.UtcNow.ToOffset(new TimeSpan(8,0,0)).Date);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string date)
        {
            try
            {
                _logger.Debug("Received {Controller} {Name} {Date}",
                    nameof(DailyHomeConsumptionController),
                    nameof(Get),
                    date);
                var home = _mediator.Send(new DailyHomeConsumptionCommand
                {
                    Date = DateTime.Parse(date)
                });
                var powerFlow = _mediator.Send(new DailyPowerMovementCommand
                {
                    Date = DateTime.Parse(date)
                });
                await Task.WhenAll(home, powerFlow);
                if (home.IsFaulted)
                    throw home.Exception;
                var response = powerFlow.IsFaulted 
                    ? new DailyHomeConsumptionDto(home.Result)
                    : new DailyHomeConsumptionDto(home.Result, powerFlow.Result);
                var planCommand = new A1PowerPlanCommand(home.Result, powerFlow.IsFaulted ? null : powerFlow.Result);
                var a1 = _mediator.Send(planCommand);
                var a1LegacySolar = _mediator.Send(new A1LegacySolarPowerPlanCommand(planCommand.HomeConsumption, planCommand.PowerMovements));
                var a1LegacySolarBattery = _mediator.Send(new A1LegacySolarWithBatteryPowerPlanCommand(planCommand.HomeConsumption, planCommand.PowerMovements));
                var middaySaver = _mediator.Send(new MiddaySaverSolarWithBatteryPowerPlanCommand(planCommand.HomeConsumption, planCommand.PowerMovements));
                await Task.WhenAll(a1, a1LegacySolar, a1LegacySolarBattery, middaySaver);
                response.MoneyPlans = new[]
                {
                    new MoneyPlan("A1", a1.IsFaulted ? -1 : a1.Result.Dollars),
                    new MoneyPlan("A1 7c solar", a1LegacySolar.IsFaulted ? -1 : a1LegacySolar.Result.Dollars),
                    new MoneyPlan("A1 7c solar battery", a1LegacySolarBattery.IsFaulted ? -1 : a1LegacySolarBattery.Result.Dollars),
                    new MoneyPlan("Midday saver", middaySaver.IsFaulted ? -1 : middaySaver.Result.Dollars),
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
