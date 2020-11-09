using Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Model
{
    public interface ISummaryRepository<T> where T : IIntervalEntity
    {
        Task<bool> AddAsync(T notification, CancellationToken cancellationToken);
    }
}
