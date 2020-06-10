using Microsoft.EntityFrameworkCore;
using ReadRepository.ReadModel;
using Repository;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReadRepository.Repositories
{
    public interface IVoltageSummaryReadRepository
    {
        Task<VoltageSummaryReadModel> Get(string key, CancellationToken cancellationToken);
    }

    public class VoltageSummaryReadRepository : IVoltageSummaryReadRepository
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
            return summary == null ? null : new VoltageSummaryReadModel
            {
                IntervalEndExcluded = summary.IntervalEndExcluded,
                IntervalStartIncluded = summary.IntervalStartIncluded,
                L1 = summary.L1,
                L2 = summary.L2,
                L3 = summary.L3,
                Version = summary.Version ?? 0
            };
        }
    }
}
