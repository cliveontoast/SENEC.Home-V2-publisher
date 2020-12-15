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
    public class EquipmentStatesSummaryRepoStore :
        INotificationHandler<PersistenceInfo<EquipmentStatesSummary>>, // TODO this should be a command, not publish
        INotificationHandler<EquipmentStatesSummary>
    {
        private readonly PersistToRepositoryFunctions<EquipmentStatesSummary, EquipmentStatesSummaryReadModel> _persistFunctions;
        
        public EquipmentStatesSummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            IAppCache cache,
            IRepoConfig config,
            IApplicationVersion versionConfig,
            IEquipmentStatesSummaryDocumentReadRepository readRepo,
            IEquipmentStatesSummaryRepository writeRepo)
        {
            _persistFunctions = new PersistToRepositoryFunctions<EquipmentStatesSummary, EquipmentStatesSummaryReadModel>(
                readRepo,
                writeRepo,
                versionConfig,
                logger,
                config,
                cache,
                "persistEquipmentStatesSummaryList",
                mediator,
                GetKeyExtensions.GetKey,
                GetKeyExtensions.GetKeyVersion2);
        }

        public async Task Handle(EquipmentStatesSummary notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }

        public async Task Handle(PersistenceInfo<EquipmentStatesSummary> notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }
    }
}
