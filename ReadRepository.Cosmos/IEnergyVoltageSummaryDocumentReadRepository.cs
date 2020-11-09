using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReadRepository.Cosmos
{
    public interface IEnergySummaryDocumentReadRepository : IDocumentReadRepository<EnergySummaryReadModel>
    {
        Task<PowerMovementSummaryReadModel> GetPowerMovement(string key, CancellationToken cancellationToken);
        Task<IEnumerable<EnergySummaryReadModel>> Fetch(DateTime date);
        Task<IEnumerable<PowerMovementSummaryReadModel>> FetchPowerMovements(DateTime date);
    }

    public interface IDocumentReadRepository<TReadModel> where TReadModel : class
    {
        Task<TReadModel?> Get(string key, CancellationToken cancellation);
    }
}