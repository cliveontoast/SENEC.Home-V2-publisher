using System;
using System.Threading.Tasks;
using Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NuanceWebApp.Dto;
using Serilog;

namespace NuanceWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyPowerMovementController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public DailyPowerMovementController(
            ILogger logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string date)
        {
            try
            {
                _logger.Debug("Received {Controller} {Name} {Date}",
                    nameof(DailyPowerMovementCommand),
                    nameof(Get),
                    date);
                var result = await _mediator.Send(new DailyPowerMovementCommand
                {
                    Date = DateTime.Parse(date)
                });
                var response = new DailyPowerMovementDto(result);
                return Ok(response);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
