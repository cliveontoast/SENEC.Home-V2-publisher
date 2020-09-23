using FroniusEntities;
using FroniusSource;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class FroniusPollCommand : IRequest<TimeSpan?>
    {
    }

    public class FroniusPollCommandHandler : IRequestHandler<FroniusPollCommand, TimeSpan?>
    {
        private readonly IGetPowerFlowRealtimeDataRequest _request;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public FroniusPollCommandHandler(
            IGetPowerFlowRealtimeDataRequest request,
            ILogger logger,
            IMediator mediator)
        {
            _request = request;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<TimeSpan?> Handle(FroniusPollCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _request.Request<GetPowerFlowRealtimeDataResponse>(cancellationToken);
                if (response.Body?.Data?.Site?.P_PV == null) return TimeSpan.FromSeconds(10);
                if (response.Head?.Timestamp == null) return TimeSpan.FromSeconds(10);

                await _mediator.Publish(
                    new SolarPower(
                        sentMilliseconds: response.SentMilliseconds,
                        receivedMilliseconds: response.ReceivedMilliseconds,
                        sourceTimestamp: response.Head.Timestamp.Value,
                        powerWatts: response.Body.Data.Site.P_PV.Value),
                    cancellationToken);
            }
            catch (BackOffPeriodException be)
            {
                _logger.Information("Fronius backoff requested {BackoffTime}", be.BackoffTime);
                return be.BackoffTime;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
                return TimeSpan.FromSeconds(30);
            }
            return null;
        }
    }
}
