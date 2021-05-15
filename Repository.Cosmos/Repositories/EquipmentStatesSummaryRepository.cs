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
    public interface IEquipmentStatesSummaryRepository : ISummaryRepository<EquipmentStatesSummary>
    {
    }

    public class EquipmentStatesSummaryRepository : IEquipmentStatesSummaryRepository
    {
        private readonly IContext _context;

        public EquipmentStatesSummaryRepository(
            IContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(EquipmentStatesSummary notification, CancellationToken cancellationToken)
        {
            if (await ReplaceItem(notification, cancellationToken))
                return true;
            return await CreateItem(notification, cancellationToken);
        }

        private async Task<bool> CreateItem(EquipmentStatesSummary notification, CancellationToken cancellationToken)
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

        private async Task<bool> ReplaceItem(EquipmentStatesSummary notification, CancellationToken cancellationToken)
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
