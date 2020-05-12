using MediatR;
using SenecEntities;
using SenecSource;
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
        private readonly IMediator _mediator;

        public SenecPollCommandHandler(
            ILalaRequestBuilder builder,
            IMediator mediator)
        {
            _builder = builder;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(SenecPollCommand request, CancellationToken cancellationToken)
        {
            var lalaRequest = _builder
                .AddGridMeter()
                .AddTime()
                .Build();

            var response = await lalaRequest.Request<LalaResponseContent>(cancellationToken);
            if (response == null) return Unit.Value;

            await _mediator.Publish(new GridMeter
            {
                PM1OBJ1 = response.PM1OBJ1,
                RTC = response.RTC,
                Sent = response.Sent,
                Received = response.Received,
            }, cancellationToken);
            return Unit.Value;
        }
    }
}
