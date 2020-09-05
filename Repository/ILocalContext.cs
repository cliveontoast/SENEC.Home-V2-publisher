using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Repository.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Repository
{
    public interface ILocalContext
    {
        DbSet<VoltageSummary> VoltageSummaries { get; }

        Task SeedAsync(CancellationToken cancellationToken);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        int SaveChanges();
    }
}