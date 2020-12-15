using Microsoft.Azure.Cosmos.Linq;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Threading;
using Repository.Model;

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

        public async Task<VoltageSummaryReadModel?> Get(string key, CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<VoltageSummary>();
            var iterator = queryable.Where(p => p.Key == key).ToFeedIterator();
            var response = await ToReadModel(iterator);
            return response.FirstOrDefault();
        }
        public async Task<IEnumerable<VoltageSummaryReadModel>> Fetch(DateTime date)
        {
            var dateText = date.ToString("yyyy-MM-dd");
            var queryable = _readContext.GetQueryable<VoltageSummary>();
            var iterator = queryable.Where(p => p.Key.StartsWith(dateText)).ToFeedIterator();
            var response = await ToReadModel(iterator);
            return response;
        }

        private static async Task<ImmutableList<VoltageSummaryReadModel>> ToReadModel(Microsoft.Azure.Cosmos.FeedIterator<VoltageSummary> iterator)
        {
            var results = await iterator.ReadNextAsync();
            var response = results.Select(a => new VoltageSummaryReadModel(
                intervalEndExcluded: a.IntervalEndExcluded,
                intervalStartIncluded: a.IntervalStartIncluded,
                key: a.Key,
                l1: a.L1,
                l2: a.L2,
                l3: a.L3,
                version: a.Version ?? 0)
                ).ToImmutableList();
            return response;
        }
    }
}
