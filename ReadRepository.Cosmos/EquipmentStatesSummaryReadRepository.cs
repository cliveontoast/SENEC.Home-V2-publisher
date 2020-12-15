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
    public interface IEquipmentStatesSummaryDocumentReadRepository : IDocumentReadRepository<EquipmentStatesSummaryReadModel>
    {
        Task<IEnumerable<EquipmentStatesSummaryReadModel>> Fetch(DateTime date);
    }

    public class EquipmentStatesSummaryDocumentReadRepository : IEquipmentStatesSummaryDocumentReadRepository
    {
        private readonly IReadContext _readContext;

        public EquipmentStatesSummaryDocumentReadRepository(
            IReadContext readContext)
        {
            _readContext = readContext;
        }

        public async Task<EquipmentStatesSummaryReadModel?> Get(string key, CancellationToken cancellationToken)
        {
            var partitionTxt = PartitionText.EQ_;
            var queryable = _readContext.GetQueryable<EquipmentStatesSummaryEntity>();
            var iterator = queryable.Where(p => p.Partition == partitionTxt + key).ToFeedIterator();
            var result = await iterator.ReadNextAsync();
            var response = ToReadModel(result);
            return response.FirstOrDefault();
        }

        public async Task<IEnumerable<EquipmentStatesSummaryReadModel>> Fetch(DateTime date)
        {
            var dateText = PartitionText.EQ_ + date.ToString("yyyy-MM-dd");
            var queryable = _readContext.GetQueryable<EquipmentStatesSummaryEntity>();
            var iterator = queryable.Where(p => p.Partition.StartsWith(dateText) && p.Discriminator == EquipmentStatesSummaryEntity.DISCRIMINATOR).ToFeedIterator();
            // SELECT* FROM c
            // where 1 = 1
            // and startswith(c.Partition, 'EQ_2020-12-14')

            var results = await iterator.ReadNextAsync();
            return ToReadModel(results);
        }

        private static ImmutableList<EquipmentStatesSummaryReadModel> ToReadModel(IEnumerable<EquipmentStatesSummaryEntity> iterator)
        {
            var response = iterator.Select(a => new EquipmentStatesSummaryReadModel(
                intervalEndExcluded: a.IntervalEndExcluded,
                intervalStartIncluded: a.IntervalStartIncluded,
                key: a.Id,
                version: a.Version,
                states: a.States.ToList(),
                secondsWithoutData: a.SecondsWithoutData)
                ).ToImmutableList();
            return response;
        }
    }
}
