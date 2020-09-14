using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReadRepository.Cosmos
{
    public interface IEnergySummaryDocumentReadRepository
    {
        Task<EnergySummaryReadModel> Get(string key, CancellationToken cancellationToken);
        Task<IEnumerable<EnergySummaryReadModel>> Fetch(DateTime date);
    }
}