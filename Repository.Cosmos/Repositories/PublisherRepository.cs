using Entities;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Cosmos.Repositories
{
    public interface IPublisherRepository
    {
        Task<bool> AddOrUpdateAsync(Publisher notification, CancellationToken cancellationToken);
    }

    public class PublisherRepository : IPublisherRepository
    {
        private readonly IContext _context;

        public PublisherRepository(
            IContext context)
        {
            _context = context;
        }

        public async Task<bool> AddOrUpdateAsync(Publisher notification, CancellationToken cancellationToken)
        {
            if (await ReplaceItem(notification, cancellationToken))
                return true;
            return await CreateItem(notification, cancellationToken);
        }

        private async Task<bool> CreateItem(Publisher notification, CancellationToken cancellationToken)
        {
            try
            {
                var createResult = await _context.CreateItemAsync(notification)(cancellationToken);
                if (createResult.StatusCode == HttpStatusCode.Created)
                    return true;
            }
            catch (Microsoft.Azure.Cosmos.CosmosException)
            {
            }
            return false;
        }

        private async Task<bool> ReplaceItem(Publisher notification, CancellationToken cancellationToken)
        {
            try
            {
                var replaceResult = await _context.ReplaceItemAsync(notification)(cancellationToken);
                if (replaceResult.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            catch (Microsoft.Azure.Cosmos.CosmosException)
            {
            }
            return false;
        }
    }
}
