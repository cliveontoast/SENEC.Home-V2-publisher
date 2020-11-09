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
            var result = await _context.CreateItemAsync(notification)(cancellationToken);
            return result.StatusCode == HttpStatusCode.Created;
        }
    }
}
