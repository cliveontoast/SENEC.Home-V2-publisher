using Entities;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Cosmos.Repositories
{
    public interface IEnergySummaryRepository : ISummaryRepository<EnergySummary>
    {
    }

    public class EnergySummaryRepository : IEnergySummaryRepository
    {
        private readonly IContext _context;

        public EnergySummaryRepository(
            IContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(EnergySummary notification, CancellationToken cancellationToken)
        {
            if (await ReplaceItem(notification, cancellationToken))
                return true;
            return await CreateItem(notification, cancellationToken);
        }

        private async Task<bool> CreateItem(EnergySummary notification, CancellationToken cancellationToken)
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

        private async Task<bool> ReplaceItem(EnergySummary notification, CancellationToken cancellationToken)
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
