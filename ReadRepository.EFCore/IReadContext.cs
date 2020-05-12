using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Model;
using System;

namespace ReadRepository
{
    public interface IReadContext
    {
        DbSet<VoltageSummary> VoltageSummaries { get; set; }
    }

    public class ReadContext : LocalContext, IReadContext
    {
        public ReadContext(Func<ILocalContextConfiguration> configuration)
            : base(configuration)
        {
        }
    }
}
