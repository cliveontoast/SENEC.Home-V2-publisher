using Repository.Model;
using Shared;
using System;
using System.Threading.Tasks;

namespace Repository
{
    public interface IVoltageSummaryRepository
    {
        void Add(Entities.VoltageSummary notification);
        Task AddAsync(Entities.VoltageSummary notification);
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
        public async Task AddAsync(Entities.VoltageSummary notification)
        {
            var c = _context;
            c.VoltageSummaries.Add(new VoltageSummary(notification, _version.Number));
            await c.SaveChangesAsync();
        }
        public void Add(Entities.VoltageSummary notification)
        {
            _context.VoltageSummaries.Add(new VoltageSummary(notification, _version.Number));
            _context.SaveChanges();
        }
    }
}
