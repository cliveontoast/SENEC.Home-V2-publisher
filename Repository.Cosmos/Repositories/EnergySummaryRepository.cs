using Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Cosmos.Repositories
{
    public interface IEnergySummaryRepository
    {
        Task<HttpStatusCode> AddAsync(EnergySummary notification, CancellationToken cancellationToken);
    }

    public class EnergySummaryRepository : IEnergySummaryRepository
    {
        private readonly IContext _context;

        public EnergySummaryRepository(
            IContext context)
        {
            _context = context;
        }
        public async Task<HttpStatusCode> AddAsync(EnergySummary notification, CancellationToken cancellationToken)
        {
            var result = await _context.CreateItemAsync(notification)(cancellationToken);
            return result.StatusCode;
        }
    }
}
