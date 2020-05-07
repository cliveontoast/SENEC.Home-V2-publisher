using Repository.Model;
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
        private readonly ILocalContext _context;

        public VoltageSummaryRepository(
            ILocalContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Entities.VoltageSummary notification)
        {
            _context.VoltageSummaries.Add(new VoltageSummary(notification));
            await _context.SaveChangesAsync();
        }
        public void Add(Entities.VoltageSummary notification)
        {
            _context.VoltageSummaries.Add(new VoltageSummary(notification));
            _context.SaveChanges();
        }
    }
}
