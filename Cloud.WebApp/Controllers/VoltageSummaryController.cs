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
    public class VoltageSummaryController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public VoltageSummaryController(
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
                    nameof(VoltageSummaryController),
                    nameof(Get),
                    date);
                var result = await _mediator.Send(new DailyVoltageSummaryCommand
                {
                    Date = DateTime.Parse(date)
                });
                var response = new DailyVoltageSummaryDto(result);
                return Ok(response);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
