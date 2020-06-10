using Microsoft.Azure.Cosmos.Linq;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Threading;

namespace ReadRepository.Cosmos
{
    public class VoltageSummaryDocumentReadRepository : IVoltageSummaryDocumentReadRepository
    {
        private readonly IReadContext _readContext;

        public VoltageSummaryDocumentReadRepository(
            IReadContext readContext)
        {
            _readContext = readContext;
        }

        public async Task<VoltageSummaryReadModel> Get(string key, CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<Repository.Model.VoltageSummary>();
            var iterator = queryable.Where(p => p.Key == key).ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            var response = results.Select(a => new VoltageSummaryReadModel
            {
                IntervalEndExcluded = a.IntervalEndExcluded,
                IntervalStartIncluded = a.IntervalStartIncluded,
                Key = a.Key,
                L1 = a.L1,
                L2 = a.L2,
                L3 = a.L3,
                Version = a.Version ?? 0,
            }).ToImmutableList();
            return response.FirstOrDefault();
        }
        public async Task<IEnumerable<VoltageSummaryReadModel>> Fetch(DateTime date)
        {
            var dateText = date.ToString("yyyy-MM-dd");
            var queryable = _readContext.GetQueryable<Repository.Model.VoltageSummary>();
            var iterator = queryable.Where(p => p.Key.StartsWith(dateText)).ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            var response = results.Select(a => new VoltageSummaryReadModel
            {
                IntervalEndExcluded = a.IntervalEndExcluded,
                IntervalStartIncluded = a.IntervalStartIncluded,
                Key = a.Key,
                L1 = a.L1,
                L2 = a.L2,
                L3 = a.L3,
                Version = a.Version ?? 0,
            }).ToImmutableList();
            return response;
        }
    }
}
