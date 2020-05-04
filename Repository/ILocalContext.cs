using Microsoft.EntityFrameworkCore;
using Repository.Model;
using System.Threading.Tasks;

namespace Repository
{
    public interface ILocalContext
    {
        DbSet<VoltageSummary> VoltageSummaries { get; set; }

        Task SeedAsync();

        Task<int> SaveChangesAsync();
    }
}