using MediatR;
using SenecEntities;
using SenecSource;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class SenecPollCommand : IRequest<Unit>
    {
    }

    public class SenecPollCommandHandler : IRequestHandler<SenecPollCommand, Unit>
    {
        private readonly ILalaRequestBuilder _builder;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public SenecPollCommandHandler(
            ILalaRequestBuilder builder,
            ILogger logger,
            IMediator mediator)
        {
            _builder = builder;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(SenecPollCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var lalaRequest = _builder
                    .AddGridMeter()
                    .AddEnergy()
                    .AddTime()
                    .Build();

                var response = await lalaRequest.Request<LalaResponseContent>(cancellationToken);
                if (response == null) return Unit.Value;

                await _mediator.Publish(
                    new GridMeter(
                        pM1OBJ1: response.PM1OBJ1,
                        rTC: response.RTC,
                        sent: response.Sent,
                        received: response.Received),
                    cancellationToken);

                await _mediator.Publish(
                    new SmartMeterEnergy(
                        eNERGY: response.ENERGY,
                        rTC: response.RTC,
                        sent: response.Sent,
                        received: response.Received),
                    cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
            }
            return Unit.Value;
        }
    }
}
