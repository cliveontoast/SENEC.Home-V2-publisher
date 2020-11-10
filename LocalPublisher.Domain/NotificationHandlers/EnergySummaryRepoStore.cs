using Entities;
using LazyCache;
using LocalPublisher.Domain.Functions;
using MediatR;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository;
using Repository.Cosmos.Repositories;
using Serilog;
using Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class EnergySummaryRepoStore :
        INotificationHandler<PersistenceInfo<EnergySummary>>, // TODO this should be a command, not publish
        INotificationHandler<EnergySummary>
    {
        private readonly PersistToRepositoryFunctions<EnergySummary, EnergySummaryReadModel> _persistFunctions;
        
        public EnergySummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            IAppCache cache,
            IRepoConfig config,
            IApplicationVersion versionConfig,
            IEnergySummaryDocumentReadRepository energySummaryReadRepository,
            IEnergySummaryRepository energySummaryRepository)
        {
            _persistFunctions = new PersistToRepositoryFunctions<EnergySummary, EnergySummaryReadModel>(
                energySummaryReadRepository,
                energySummaryRepository,
                versionConfig,
                logger,
                config,
                cache,
                "persistEnergySummaryList",
                mediator,
                GetKeyExtensions.GetKey,
                GetKeyExtensions.GetKeyVersion2);
        }

        public async Task Handle(EnergySummary notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }

        public async Task Handle(PersistenceInfo<EnergySummary> notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }
    }
}
