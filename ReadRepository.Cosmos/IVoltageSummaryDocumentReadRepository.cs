using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReadRepository.Cosmos
{
    public interface IVoltageSummaryDocumentReadRepository
    {
        Task<VoltageSummaryReadModel> Get(string key, CancellationToken cancellationToken);
        Task<IEnumerable<VoltageSummaryReadModel>> Fetch(DateTime date);
    }
}