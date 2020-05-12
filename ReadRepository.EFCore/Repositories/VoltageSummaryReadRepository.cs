using ReadRepository.ReadModel;
using Repository;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadRepository.Repositories
{
    public interface IVoltageSummaryReadRepository
    {
        VoltageSummaryReadModel Get(string key);
    }

    public class VoltageSummaryReadRepository : IVoltageSummaryReadRepository
    {
        private readonly IReadContext _context;

        public VoltageSummaryReadRepository(
            IReadContext context)
        {
            _context = context;
        }


        public VoltageSummaryReadModel Get(string key)
        {
            var summary = _context.VoltageSummaries.Find(key);
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
