using Repository.Model;
using Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repository
{
    public interface IVoltageSummaryRepository : ISummaryRepository<Entities.VoltageSummary>
    {
    }

    public class VoltageSummaryRepository : IVoltageSummaryRepository
    {
        private readonly IApplicationVersion _version;
        private readonly ILocalContext _context;

        public VoltageSummaryRepository(
            IApplicationVersion version,
            ILocalContext context)
        {
            _version = version;
            _context = context;
        }
        public async Task<bool> AddAsync(Entities.VoltageSummary notification, CancellationToken cancellationToken)
        {
            var c = _context;
            await c.VoltageSummaries.AddAsync(new VoltageSummary(notification, _version.Number), cancellationToken);
            return await c.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
