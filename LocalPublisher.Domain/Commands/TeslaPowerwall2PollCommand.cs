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
    public class TeslaPowerwall2PollCommand : IRequest<TimeSpan?>
    {
    }

    public class TeslaPowerwall2PollCommandHandler : IRequestHandler<TeslaPowerwall2PollCommand, TimeSpan?>
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

        public async Task<TimeSpan?> Handle(TeslaPowerwall2PollCommand request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                _logger.Information("Cancellation requested and acknowledged. not fetching");
            try
            {
                var oneSecondSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                var energy = _apiRequest.GetMetersAggregatesAsync(oneSecondSource.Token);
                var charge = _apiRequest.GetStateOfEnergyAsync(oneSecondSource.Token);

                await Task.WhenAll(energy, charge);
                if (oneSecondSource.IsCancellationRequested || cancellationToken.IsCancellationRequested)
                {
                    _apiRequest.Destroy(cancellationToken);
                    return TimeSpan.FromSeconds(60);
                }

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
                    return TimeSpan.FromSeconds(60);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
                return TimeSpan.FromSeconds(60);
            }
            return null;
        }
    }
}
