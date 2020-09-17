using System;
using System.Threading.Tasks;
using Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Client.WebApp.Dto;
using Serilog;

namespace Client.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergySummaryController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public EnergySummaryController(
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
                    nameof(EnergySummaryController),
                    nameof(Get),
                    date);
                var result = await _mediator.Send(new DailyEnergySummaryCommand
                {
                    Date = DateTime.Parse(date)
                });
                var response = new DailyEnergySummaryDto(result);
                return Ok(response);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
