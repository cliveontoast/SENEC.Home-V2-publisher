using Autofac;
using Domain;
using LazyCache;
using LazyCache.Providers;
using LocalPublisherWebApp;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using ReadRepository.Cosmos;
using Repository;
using Repository.Cosmos;
using Repository.Cosmos.Repositories;
using SenecEntitesAdapter;
using SenecEntities;
using SenecEntitiesAdapter;
using SenecSource;
using System;
using System.Linq;
using System.Threading;

namespace SenecSourceWebAppTest
{
    [TestClass]
    public class CopyContainerData
    {
        public ILifetimeScope scope = Mock.Of<ILifetimeScope>();

        public void InitScope(Action<(ContainerBuilder, Mock<IConfiguration>)>? extras = null)
        {
            var cb = UnitTest1.Builder();
            extras?.Invoke(cb);
            var container = cb.cb.Build();
            scope = container.BeginLifetimeScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            scope?.Dispose();
        }
        [TestMethod]
        public void FetchEnergy()
        {
            InitScope(a =>
                a.Item1.RegisterModule(new ReadRepository.Cosmos.AutofacModule(a.Item2.Object))
            );
            var readRepo = scope.Resolve<IEnergySummaryDocumentReadRepository>() as EnergySummaryDocumentReadRepository;
            if (readRepo == null) return;

            var items = readRepo.FetchRaw(new DateTime(2020, 09, 15)).RunWait();
            //var items2 = readRepo.FetchRaw(new DateTime(2020, 09, 14)).RunWait();

            var writeRepo = scope.Resolve<IEnergySummaryRepository>();

            foreach (var item in items.OrderBy(a => a.Partition))
            {
                try
                {
                    writeRepo.AddAsync(item, CancellationToken.None).RunWait();
                }
                catch (AggregateException aex) when (aex.InnerException is Microsoft.Azure.Cosmos.CosmosException ce
                    && ce.StatusCode == System.Net.HttpStatusCode.Conflict)
                { 
                }
                catch (Exception e)
                {
                    var ex = e;
                }
            }
        }
    }

}
