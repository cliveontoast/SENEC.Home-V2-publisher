using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReadRepository.Cosmos
{
    public interface IVoltageSummaryDocumentReadRepository : IDocumentReadRepository<VoltageSummaryReadModel>
    {
        Task<IEnumerable<VoltageSummaryReadModel>> Fetch(DateTime date);
    }
}