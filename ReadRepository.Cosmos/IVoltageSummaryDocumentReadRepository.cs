using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadRepository.Cosmos
{
    public interface IVoltageSummaryDocumentReadRepository
    {
        Task<IEnumerable<VoltageSummaryReadModel>> Fetch(DateTime date);
    }
}