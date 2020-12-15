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
            var result = await _context.CreateItemAsync(notification)(cancellationToken);
            return result.StatusCode == HttpStatusCode.Created;
        }
    }
}
