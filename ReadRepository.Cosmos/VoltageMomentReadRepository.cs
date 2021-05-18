using Microsoft.Azure.Cosmos.Linq;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Threading;
using Repository.Model;
using Entities;
using Newtonsoft.Json;

namespace ReadRepository.Cosmos
{
    public interface IVoltageMomentReadRepository : IDocumentReadRepository<VoltageMomentReadModel>
    {
        Task<IEnumerable<VoltageMomentReadModel>> Fetch(DateTime date);
    }

    public class VoltageMomentReadRepository : IVoltageMomentReadRepository
    {
        private readonly IReadContext _readContext;

        public VoltageMomentReadRepository(
            IReadContext readContext)
        {
            _readContext = readContext;
        }

        public async Task<VoltageMomentReadModel?> Get(string key, CancellationToken cancellationToken)
        {
            var date = JsonConvert.DeserializeObject<DateTimeOffset>("\"" + key + "\"");
            key = IntervalOfMomentsEntity<MomentVoltage>.GetRepositoryKey(date);
            var partitionTxt = PartitionText.MV_;
            var queryable = _readContext.GetQueryable<IntervalOfMomentsEntity<VoltageMomentEntity>>();
            var iterator = queryable.Where(p => p.Key == partitionTxt + key).ToFeedIterator();
            var result = await iterator.ReadNextAsync();
            var response = ToReadModel(result);
            var readModel = response.FirstOrDefault();
            var item = readModel?.IntervalStartIncluded == date
                ? readModel : default;
            return item;
        }



        public async Task<IEnumerable<VoltageMomentReadModel>> Fetch(DateTime date)
        {
            var dateText = PartitionText.MV_ + date.DayOfWeek.ToString();
            var queryable = _readContext.GetQueryable<IntervalOfMomentsEntity<VoltageMomentEntity>>();
            var iterator = queryable.Where(p => p.Key.StartsWith(dateText)
            //&& p.Discriminator == IntervalOfMomentsEntity<VoltageMomentEntity>.DISCRIMINATOR
            ).ToFeedIterator();
            // SELECT* FROM c
            // where 1 = 1
            // and startswith(c.Partition, 'MV_Sunday')

            var results = await iterator.ReadNextAsync();
            return ToReadModel(results);
        }

        private static ImmutableList<VoltageMomentReadModel> ToReadModel(IEnumerable<IntervalOfMomentsEntity<VoltageMomentEntity>> iterator)
        {
            var response = iterator.Select(a => new VoltageMomentReadModel(
                intervalEndExcluded: a.IntervalEndExcluded,
                intervalStartIncluded: a.IntervalStartIncluded,
                key: a.Id, 
                version: a.Version,
                moments: a.Moments.Select(a => (MomentVoltage)a))
                ).ToImmutableList();
            return response;
        }
    }
}
