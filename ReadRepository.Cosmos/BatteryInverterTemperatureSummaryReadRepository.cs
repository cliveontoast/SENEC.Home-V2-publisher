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

        public async Task<BatteryInverterTemperatureSummaryReadModel?> Get(string key, CancellationToken cancellationToken)
        {
            var discriminator = PartitionText.ITS_.ToString();
            var queryable = _readContext.GetQueryable<BatteryInverterTemperatureSummaryEntity>();
            var iterator = queryable.Where(p => p.Partition == discriminator + key).ToFeedIterator();
            var result = await iterator.ReadNextAsync();
            var response = ToReadModel(result);
            return response.FirstOrDefault();
        }

        public async Task<IEnumerable<BatteryInverterTemperatureSummaryReadModel>> Fetch(DateTime date)
        {
            var dateText = PartitionText.ITS_.ToString() + date.ToString("yyyy-MM-dd");
            var queryable = _readContext.GetQueryable<BatteryInverterTemperatureSummaryEntity>();
            var iterator = queryable.Where(p => p.Partition.StartsWith(dateText) && p.Discriminator == BatteryInverterTemperatureSummaryEntity.DISCRIMINATOR).ToFeedIterator();
            // should convert to cosmos-sql 
            // SELECT * FROM c
            // where startswith(c.id, '2020-11-14')

            var results = await iterator.ReadNextAsync();
            return ToReadModel(results);
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
