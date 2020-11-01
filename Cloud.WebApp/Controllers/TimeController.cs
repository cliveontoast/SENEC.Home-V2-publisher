using System;
using System.Threading.Tasks;
using Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NuanceWebApp.Dto;
using Serilog;
using Shared;

namespace NuanceWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IZoneProvider _zoneProvider;
        private readonly IMediator _mediator;

        public TimeController(
            ILogger logger,
            ITimeProvider timeProvider,
            IZoneProvider zoneProvider,
            IMediator mediator)
        {
            _logger = logger;
            _timeProvider = timeProvider;
            _zoneProvider = zoneProvider;
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult Get(string date)
        {
            try
            {
                _logger.Debug("Received {Controller} {Name}",
                    nameof(TimeController),
                    nameof(Get));
                return Ok(_timeProvider.Now.ToEquipmentLocalTime(_zoneProvider));
            }
            catch (Exception e)
            {
                return this.Problem(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
