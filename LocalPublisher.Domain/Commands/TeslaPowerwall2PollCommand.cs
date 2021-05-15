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
            if (cancellationToken.IsCancellationRequested)
                _logger.Information("Cancellation requested and acknowledged. not fetching");
            try
            {
                var energy = _apiRequest.GetMetersAggregatesAsync(cancellationToken);
                var charge = _apiRequest.GetStateOfEnergyAsync(cancellationToken);

                await Task.WhenAll(energy, charge);

                if (!energy.IsFaulted)
                {
                    var start = DateTimeOffset.FromUnixTimeMilliseconds(energy.Result.SentMilliseconds).LocalDateTime;
                    var end = DateTimeOffset.FromUnixTimeMilliseconds(energy.Result.ReceivedMilliseconds).LocalDateTime;
                    _logger.Information(Client.LOGGING + $"Received energy ${energy.Result.site?.last_communication_time} r: {end} s: {start}");
                    await _mediator.Publish(new MetersAggregatesItem(energy.Result), cancellationToken);
                }
                if (!charge.IsFaulted)
                {
                    await _mediator.Publish(new StateOfEnergyItem(charge.Result), cancellationToken);
                }
            }
            catch (TaskCanceledException e)
            {
                _apiRequest.Destroy(cancellationToken);
                _logger.Error(e, "Cancelled task");
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
            }
            return Unit.Value;
        }
    }
}
