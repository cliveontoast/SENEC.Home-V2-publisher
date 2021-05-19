using MediatR;
using SenecEntities;
using SenecEntitiesAdapter;
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
        private readonly IAdapter _adapter;
        private readonly IMediator _mediator;

        public SenecPollCommandHandler(
            ILalaRequestBuilder builder,
            ILogger logger,
            IAdapter adapter,
            IMediator mediator)
        {
            _builder = builder;
            _logger = logger;
            _adapter = adapter;
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
                    .AddInverterTemperature()
                    .AddTemperatureMeasure()
                    .Build();

                var response = await lalaRequest.Request<LalaResponseContent>(cancellationToken);
                if (response.RTC == null) return Unit.Value;
                var sourceTime = _adapter.GetDecimal(response.RTC.WEB_TIME).AsInteger;
                if (response.PM1OBJ1 != null)
                {
                    await _mediator.Publish(
                        new GridMeter(
                            meter: response.PM1OBJ1,
                            sourceTimestamp: response.RTC,
                            sentMilliseconds: response.SentMilliseconds,
                            receivedMilliseconds: response.ReceivedMilliseconds),
                        cancellationToken);
                }
                if (response.ENERGY != null)
                {
                    await _mediator.Publish(
                        new SmartMeterEnergy(
                            energy: response.ENERGY,
                            sourceTimestamp: response.RTC,
                            sentMilliseconds: response.SentMilliseconds,
                            receivedMilliseconds: response.ReceivedMilliseconds),
                        cancellationToken);
                }
                if (response.BAT1OBJ1 != null)
                {
                    await _mediator.Publish(
                        new BatteryInverterTemperature(
                            inverter: response.BAT1OBJ1,
                            other: response.TEMPMEASURE,
                            sourceTimestamp: response.RTC,
                            sentMilliseconds: response.SentMilliseconds,
                            receivedMilliseconds: response.ReceivedMilliseconds),
                        cancellationToken);
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
