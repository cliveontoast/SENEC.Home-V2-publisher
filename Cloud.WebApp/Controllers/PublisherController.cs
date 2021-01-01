using System;
using System.Threading.Tasks;
using Client.WebApp.Dto;
using Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared;

namespace NuanceWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IZoneProvider _zoneProvider;
        private readonly IMediator _mediator;

        public PublisherController(
            ILogger logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                _logger.Debug("Received {Controller} {Name}",
                    nameof(PublisherController),
                    nameof(Get));
                var result = await _mediator.Send(new PublishersCommand());
                var response = new PublishersDto(result);
                return Ok(response);
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
