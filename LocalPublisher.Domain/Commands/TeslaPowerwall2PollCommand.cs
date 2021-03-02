using MediatR;
using SenecEntities;
using SenecEntitiesAdapter;
using SenecSource;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;
using TeslaPowerwallSource;

namespace Domain
{
    public class TeslaPowerwall2PollCommand : IRequest<Unit>
    {
    }

    public class TeslaPowerwall2PollCommandHandler : IRequestHandler<TeslaPowerwall2PollCommand, Unit>
    {
        private readonly IApiRequest _apiRequest;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public TeslaPowerwall2PollCommandHandler(
            IApiRequest apiRequest,
            ILogger logger,
            IMediator mediator)
        {
            _apiRequest = apiRequest;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(TeslaPowerwall2PollCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var energy = _apiRequest.GetMetersAggregatesAsync(cancellationToken);
                var charge = _apiRequest.GetStateOfEnergyAsync(cancellationToken);

                await Task.WhenAll(energy, charge);

                if (!energy.IsFaulted)
                {
                    await _mediator.Publish(new MetersAggregatesItem(energy.Result), cancellationToken);
                }
                if (!charge.IsFaulted)
                {
                    await _mediator.Publish(new StateOfEnergyItem(charge.Result), cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
            }
            return Unit.Value;
        }
    }
}
