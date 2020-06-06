using Entities;
using MediatR;
using Newtonsoft.Json;
using ReadRepository.Repositories;
using Repository.Model;
using Serilog;
using Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.NotificationHandlers
{
    public class PersistErrorCommand<TEntity> : IRequest<Unit>
    {
        public TEntity Entity { get; set; }
        public HttpStatusCode Response { get; set; }
    }

    public class VoltageSummaryPersistErrorCommand : IRequestHandler<PersistErrorCommand<Entities.VoltageSummary>>
    {
        private readonly IVoltageSummaryReadRepository _voltageSummaryReadRepository;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IApplicationVersion _version;

        public VoltageSummaryPersistErrorCommand(
            IVoltageSummaryReadRepository voltageSummaryReadRepository,
            IMediator mediator,
            ILogger logger,
            IApplicationVersion version)
        {
            _voltageSummaryReadRepository = voltageSummaryReadRepository;
            _mediator = mediator;
            _logger = logger;
            _version = version;
        }

        public async Task<Unit> Handle(PersistErrorCommand<Entities.VoltageSummary> request, CancellationToken cancellationToken)
        {
            var retryItem = request.Entity as VoltageSummaryRetry;
            if (retryItem == null)
            {
                retryItem = JsonConvert.DeserializeObject<VoltageSummaryRetry>(JsonConvert.SerializeObject(request.Entity));
            }
            retryItem.Increment();
            var delay = (int)Math.Pow(2, retryItem.GetRetryCount()) * 10000;

            _logger.Error("Waiting {Seconds} before publishing {@Summary}", delay / 1000, retryItem);
            await Task.Delay(delay, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return Unit.Value;

            if (retryItem.GetRetryCount() >= 5)
            {
                _logger.Information("Waited long enough {@Summary}", retryItem);
            }
            else
            {
                await _mediator.Publish(retryItem, cancellationToken);
            }
            return Unit.Value;
        }
    }
}
