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
    public class PublisherRepoStore :
        INotificationHandler<Publisher>
    {
        private readonly ILogger _logger;
        private IPublisherRepository _publisherRepository;

        public PublisherRepoStore(
            ILogger logger,
            IPublisherRepository publisherRepository)
        {
            _logger = logger;
            _publisherRepository = publisherRepository;
        }

        public async Task Handle(Publisher notification, CancellationToken cancellationToken)
        {
            var result = await _publisherRepository.AddOrUpdateAsync(notification, cancellationToken);
            if (result)
                _logger.Information("Publisher written successfully {@PublishMsg}", notification);
            else
                _logger.Error("Publisher failed to write {@PublishMsg}", notification);
        }
    }
}
