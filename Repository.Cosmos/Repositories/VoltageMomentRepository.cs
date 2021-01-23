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
    public interface IVoltageMomentRepository : ISummaryRepository<IntervalOfMoments<MomentVoltage>>
    {
        //Task<bool> AddOrUpdateAsync(IntervalOfMoments<MomentVoltage> notification, CancellationToken cancellationToken);
    }

    public class VoltageMomentRepository : IVoltageMomentRepository
    {
        private readonly IContext _context;

        public VoltageMomentRepository(
            IContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(IntervalOfMoments<MomentVoltage> notification, CancellationToken cancellationToken)
        {
            return await AddOrUpdateAsync(notification, cancellationToken);
        }

        public async Task<bool> AddOrUpdateAsync(IntervalOfMoments<MomentVoltage> notification, CancellationToken cancellationToken)
        {
            if (await ReplaceItem(notification, cancellationToken))
                return true;
            return await CreateItem(notification, cancellationToken);
        }

        private async Task<bool> CreateItem(IntervalOfMoments<MomentVoltage> notification, CancellationToken cancellationToken)
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

        private async Task<bool> ReplaceItem(IntervalOfMoments<MomentVoltage> notification, CancellationToken cancellationToken)
        {
            try
            {
                var replaceResult = await _context.ReplaceItemAsync(notification)(cancellationToken);
                if (replaceResult.StatusCode == HttpStatusCode.OK)
                    return true;
                else
                    return false;
            }
            catch (Microsoft.Azure.Cosmos.CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
