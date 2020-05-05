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
        private ILifetimeScope scope;

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
                .AddStatistics()
                .AddTime()
                .Build();
            var response = result.Request<LalaResponseContent>(CancellationToken.None).RunWait();
        }

        public static ContainerBuilder Builder()
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
            return cb;
        }

        public static IAppCache BuildAppCache()
        {
            return new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }

        private void InitScope(Action<ContainerBuilder> extras = null)
        {
            var cb = Builder();
            extras?.Invoke(cb);
            var container = cb.Build();
            scope = container.BeginLifetimeScope();
        }

        [TestMethod]
        public void Poll()
        {
            InitScope();

            var builder = scope.Resolve<ILalaRequestBuilder>();
            var result = builder
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
    }
}
