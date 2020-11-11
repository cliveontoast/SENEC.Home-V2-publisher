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
    public interface IInverterTemperatureSummaryDocumentReadRepository : IDocumentReadRepository<BatteryInverterTemperatureSummaryReadModel>
    {
        Task<IEnumerable<BatteryInverterTemperatureSummaryReadModel>> Fetch(DateTime date);
    }

    public class BatteryInverterTemperatureSummaryDocumentReadRepository : IInverterTemperatureSummaryDocumentReadRepository
    {
        private readonly IReadContext _readContext;

        public BatteryInverterTemperatureSummaryDocumentReadRepository(
            IReadContext readContext)
        {
            _readContext = readContext;
        }

        public async Task<BatteryInverterTemperatureSummaryReadModel> Get(string key, CancellationToken cancellationToken)
        {
            var queryable = _readContext.GetQueryable<BatteryInverterTemperatureSummaryEntity>();
            var iterator = queryable.Where(p => p.Partition == "ITS_" + key).ToFeedIterator();
            var result = await iterator.ReadNextAsync();
            var response = ToReadModel(result);
            return response.FirstOrDefault();
        }

        public Task<IEnumerable<BatteryInverterTemperatureSummaryReadModel>> Fetch(DateTime date)
        {
            throw new NotImplementedException();
        }

        private static ImmutableList<BatteryInverterTemperatureSummaryReadModel> ToReadModel(IEnumerable<BatteryInverterTemperatureSummaryEntity> iterator)
        {
            var response = iterator.Select(a => new BatteryInverterTemperatureSummaryReadModel(
                intervalEndExcluded: a.IntervalEndExcluded,
                intervalStartIncluded: a.IntervalStartIncluded,
                key: a.Id,
                version: a.Version,
                maximumTemperature: a.Temperatures,
                secondsWithoutData: a.SecondsWithoutData)
                ).ToImmutableList();
            return response;
        }
    }
}
