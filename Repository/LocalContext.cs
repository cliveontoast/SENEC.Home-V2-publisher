﻿using Microsoft.EntityFrameworkCore;
using Repository.Model;
using System;
using System.Threading.Tasks;

namespace Repository
{
    public class LocalContext : DbContext, ILocalContext
    {
        private readonly Func<ILocalContextConfiguration> _configuration;
        public DbSet<VoltageSummary> VoltageSummaries { get; set; }

        public LocalContext(
            Func<ILocalContextConfiguration> configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        public async Task SeedAsync()
        {
            //await Database.EnsureDeletedAsync();
            await Database.EnsureCreatedAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = _configuration();
            optionsBuilder.UseCosmos(
                config.AccountEndPoint,
                config.AccountKey,
                config.DatabaseName);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var config = _configuration();
            modelBuilder.HasDefaultContainer(config.DefaultContainer);

            modelBuilder.Entity<VoltageSummary>()
                .HasPartitionKey(o => o.Key)
                .HasKey(o => o.Key);
        }
    }
}
