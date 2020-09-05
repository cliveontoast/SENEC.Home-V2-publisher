using ReadRepository.ReadModel;
using System.Threading;
using System.Threading.Tasks;

namespace ReadRepository.Repositories
{
    // todo delete
    internal interface IVoltageSummaryReadRepository
    {
        Task<VoltageSummaryReadModel> Get(string key, CancellationToken cancellationToken);
    }
    // todo delete
    internal class VoltageSummaryReadRepository : IVoltageSummaryReadRepository
    {
        private readonly IReadContext _context;

        public VoltageSummaryReadRepository(
            IReadContext context)
        {
            _context = context;
        }

        public async Task<VoltageSummaryReadModel> Get(string key, CancellationToken cancellationToken)
        {
            var summary = await _context.VoltageSummaries.FindAsync(key, cancellationToken);
            return summary == null ? null : new VoltageSummaryReadModel(
                intervalEndExcluded: summary.IntervalEndExcluded,
                intervalStartIncluded: summary.IntervalStartIncluded,
                key: summary.Key,
                l1: summary.L1,
                l2: summary.L2,
                l3: summary.L3,
                version: summary.Version ?? 0);
        }
    }
}
