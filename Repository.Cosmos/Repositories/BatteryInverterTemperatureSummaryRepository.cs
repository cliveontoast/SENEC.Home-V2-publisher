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
    public interface IBatteryInverterTemperatureSummaryRepository : ISummaryRepository<InverterTemperatureSummary>
    {
    }

    public class BatteryInverterTemperatureSummaryRepository : IBatteryInverterTemperatureSummaryRepository
    {
        private readonly IContext _context;

        public BatteryInverterTemperatureSummaryRepository(
            IContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(InverterTemperatureSummary notification, CancellationToken cancellationToken)
        {
            var result = await _context.CreateItemAsync(notification)(cancellationToken);
            return result.StatusCode == HttpStatusCode.Created;
        }
    }
}
