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
                var result = await _mediator.Send(new DailyHomeConsumptionCommand
                {
                    Date = DateTime.Parse(date)
                });
                var response = new DailyHomeConsumptionDto(result);
                return Ok(response);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
