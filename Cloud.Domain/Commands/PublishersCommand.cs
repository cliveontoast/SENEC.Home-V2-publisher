using Entities;
using LazyCache;
using MediatR;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class PublishersCommand : IRequest<IEnumerable<PublisherReadModel>>
    {
    }

    public class PublishersCommandHandler : IRequestHandler<PublishersCommand, IEnumerable<PublisherReadModel>>
    {
        private readonly IAppCache _appCache;
        private readonly IPublisherDocumentReadRepository _publisherDocumentReadRepository;

        public PublishersCommandHandler(
            IAppCache appCache,
            IPublisherDocumentReadRepository publisherDocumentReadRepository)
        {
            _appCache = appCache;
            _publisherDocumentReadRepository = publisherDocumentReadRepository;
        }

        public async Task<IEnumerable<PublisherReadModel>> Handle(PublishersCommand request, CancellationToken cancellationToken)
        {
            var results = await _appCache.GetOrAddAsync("publishers", async () => await _publisherDocumentReadRepository.Fetch(cancellationToken), TimeSpan.FromMinutes(5));
            return results;
        }
    }
}
