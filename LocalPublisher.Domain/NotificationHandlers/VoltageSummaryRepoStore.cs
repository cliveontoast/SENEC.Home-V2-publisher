using Entities;
using LazyCache;
using LocalPublisher.Domain.Functions;
using MediatR;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository;
using Serilog;
using Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageSummaryRepoStore :
        INotificationHandler<PersistenceInfo<VoltageSummary>>, // TODO this should be a command, not publish
        INotificationHandler<VoltageSummary>
    {
        private readonly PersistToRepositoryFunctions<VoltageSummary, VoltageSummaryReadModel> _persistFunctions;

        public VoltageSummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            IAppCache cache,
            IRepoConfig config,
            IApplicationVersion versionConfig,
            IVoltageSummaryDocumentReadRepository voltageSummaryReadRepository,
            IVoltageSummaryRepository voltageSummaryRepository)
        {
            _persistFunctions = new PersistToRepositoryFunctions<VoltageSummary, VoltageSummaryReadModel>(
                voltageSummaryReadRepository,
                voltageSummaryRepository,
                versionConfig,
                logger,
                config,
                cache,
                "persistVoltageSummaryList",
                mediator,
                GetKeyExtensions.GetKey,
                GetKeyExtensions.GetKeyVersion2);
        }

        public async Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }

        public async Task Handle(PersistenceInfo<VoltageSummary> notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }
    }
}
