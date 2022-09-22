using Microsoft.Azure.Cosmos.Linq;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Repository.Model;
using Repository;

namespace ReadRepository.Cosmos
{
    public interface IPublisherDocumentReadRepository
    {
        Task<IEnumerable<PublisherReadModel>> Fetch(CancellationToken cancellationToken);
    }

    public class PublisherDocumentReadRepository : IPublisherDocumentReadRepository
    {
        private readonly IReadContext _readContext;

        public PublisherDocumentReadRepository(
            IReadContext readContext)
        {
            _readContext = readContext;
        }

        public async Task<IEnumerable<PublisherReadModel>> Fetch(CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<PublisherEntity>();
            var iterator = queryable.Where(p => p.Partition == PartitionText.PU_.ToString() && p.Discriminator == PublisherEntity.DISCRIMINATOR).ToFeedIterator();
            // SELECT* FROM c
            // where 1 = 1
            // and startswith(c.Partition, 'PU')

            var results = await iterator.ReadNextAsync(cancellationToken);
            return ToReadModel(results);
        }

        private static PublisherReadModel[] ToReadModel(IEnumerable<PublisherEntity> iterator)
        {
            var response = iterator.Select(a => new PublisherReadModel(
                key: a.GetKey(),
                name: a.Name,
                lastActive: a.LastActive)
                ).ToArray();
            return response;
        }
    }
}
