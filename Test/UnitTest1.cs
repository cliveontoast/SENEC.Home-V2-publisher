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
using ReadRepository.Cosmos;
using ReadRepository.Repositories;
using Repository;
using SenecEntitesAdapter;
using SenecEntities;
using SenecEntitiesAdapter;
using SenecSource;
using System;
using System.Threading;

namespace SenecSourceWebAppTest
{
    [TestClass]
    public class UnitTest1
    {
        public ILifetimeScope scope = Mock.Of<ILifetimeScope>();

        [TestCleanup]
        public void Cleanup()
        {
            scope?.Dispose();
        }

        [TestMethod]
        public void TestMethod1()
        {
            //var one = new Lala1();
            //var response1 = one.Request(CancellationToken.None);
            //var two = new Lala2();
            //var response2 = two.Request(CancellationToken.None);

            //var result1 = response1.RunWait();
            //var result2 = response2.RunWait();
        }

        [TestMethod]
        public void FloatTest()
        {
            string freq = "fl_42483333";
            string volt = "fl_436FCCCD";
            var adapter = new Adapter();
            var result1 = adapter.GetValue(freq);
            var result2 = adapter.GetValue(volt);
            var result3 = adapter.GetValue("u8_0E");
            var result4 = adapter.GetValue(null);
        }

        //[TestMethod]
        //public void RequestBuilder()
        //{
        //    LalaRequestBuilder builder = new LalaRequestBuilder();
        //    var result = builder.AddConsumption().RunWait()
        //        .Build();
        //}

        [TestMethod]
        public void RequestBuilder_GridMeter()
        {
            InitScope();

            var builder = scope.Resolve<ILalaRequestBuilder>();
            var result = builder
                .AddGridMeter()
                .AddTime()
                .Build();
            var response = result.Request<LalaResponseContent>(CancellationToken.None).RunWait();
        }

        [TestMethod]
        public void RequestBuilder_DailyStats()
        {
            InitScope();

            var builder = scope.Resolve<ILalaRequestBuilder>();
            var result = builder
                // returns variable not found
                .AddStatistics()

                .AddEnergy()
                .AddTime()
                .Build();
            var response = result.Request<LalaResponseContent>(CancellationToken.None).RunWait();

            var senecAdapter = new Adapter();
            var time = senecAdapter.GetDecimal(response?.RTC?.WEB_TIME);
            var adapter = new EnergyAdapter(new Adapter());

            if (response?.ENERGY == null || !time.Value.HasValue)
                return;
            var energy = adapter.Convert((long)time.Value.Value, response.ENERGY);
            var ename = energy.SystemState.EnglishName;
        }

        public static (ContainerBuilder cb, Mock<IConfiguration> conf) Builder()
        {
            Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();

            IServiceCollection services = new ServiceCollection();
            var cb = AssemblySetup.BuildContainer(services);

            var startup = new Startup(mockConfiguration.Object);
            startup.ConfigureServices(services);
            cb.RegisterInstance(BuildAppCache());
            startup.ConfigureContainer(cb);
            cb.RegisterInstance<ISenecSettings>(new SenecSettings
            {
                IP = "192.168.0.199"
            }).SingleInstance();
            cb.RegisterInstance<ILocalContextConfiguration>(new LocalContextConfiguration(
                accountEndPoint: "https://....documents.azure.com:443/",
                accountKey: "...",
                defaultContainer: "SenecDev",
                databaseName: "ToDoList"));
            return (cb, mockConfiguration);
        }

        public static IAppCache BuildAppCache()
        {
            return new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }

        public void InitScope(Action<(ContainerBuilder, Mock<IConfiguration>)>? extras = null)
        {
            var cb = Builder();
            extras?.Invoke(cb);
            var container = cb.cb.Build();
            scope = container.BeginLifetimeScope();
        }

        [TestMethod]
        public void Poll()
        {
            InitScope();

            var builder = scope.Resolve<ILalaRequestBuilder>();
            var result = builder
                .AddEnergy()
                .AddGridMeter()
                .AddTime()
                .Build();
            scope.RunWait(new SenecPollCommand());
        }

        [TestMethod]
        public void Convert()
        {
            var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
            var a = new Repository.Model.VoltageSummary()
            {
                IntervalStartIncluded = now,
            };
            Assert.AreEqual(now, a.IntervalStartIncluded);
            var s = a.Key;
            a.Key = s;
            Assert.AreEqual(now, a.IntervalStartIncluded);
        }



        [TestMethod]
        public void Fetch()
        {
            InitScope(a =>
                a.Item1.RegisterModule(new ReadRepository.Cosmos.AutofacModule(a.Item2.Object))
            );
            var readRepo = scope.Resolve<IVoltageSummaryDocumentReadRepository>();
            readRepo.Fetch(new DateTime(2020, 05, 11)).RunWait();
        }
    }
}
