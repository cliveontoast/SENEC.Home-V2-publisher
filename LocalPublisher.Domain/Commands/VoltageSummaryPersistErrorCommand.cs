using Entities;
using MediatR;
using ReadRepository.Repositories;
using Repository.Model;
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
        private readonly IApplicationVersion _version;

        public VoltageSummaryPersistErrorCommand(
            IVoltageSummaryReadRepository voltageSummaryReadRepository,
            IApplicationVersion version)
        {
            _voltageSummaryReadRepository = voltageSummaryReadRepository;
            _version = version;
        }

        public Task<Unit> Handle(PersistErrorCommand<Entities.VoltageSummary> request, CancellationToken cancellationToken)
        {
            // TODO
            return Unit.Task;
        }
    }
}
