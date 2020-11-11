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
    public class BatteryInverterTemperatureSummaryRepoStore :
        INotificationHandler<PersistenceInfo<InverterTemperatureSummary>>, // TODO this should be a command, not publish
        INotificationHandler<InverterTemperatureSummary>
    {
        private readonly PersistToRepositoryFunctions<InverterTemperatureSummary, BatteryInverterTemperatureSummaryReadModel> _persistFunctions;
        
        public BatteryInverterTemperatureSummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            IAppCache cache,
            IRepoConfig config,
            IApplicationVersion versionConfig,
            IInverterTemperatureSummaryDocumentReadRepository readRepo,
            IBatteryInverterTemperatureSummaryRepository writeRepo)
        {
            _persistFunctions = new PersistToRepositoryFunctions<InverterTemperatureSummary, BatteryInverterTemperatureSummaryReadModel>(
                readRepo,
                writeRepo,
                versionConfig,
                logger,
                config,
                cache,
                "persistBatteryInverterTemperatureSummaryList",
                mediator,
                GetKeyExtensions.GetKey,
                GetKeyExtensions.GetKeyVersion2);
        }

        public async Task Handle(InverterTemperatureSummary notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }

        public async Task Handle(PersistenceInfo<InverterTemperatureSummary> notification, CancellationToken cancellationToken)
        {
            await _persistFunctions.Handle(notification, cancellationToken);
        }
    }
}
