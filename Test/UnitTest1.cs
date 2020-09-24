using Autofac;
using Domain;
using FroniusEntities;
using FroniusSource;
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
using SenecEntitesAdapter;
using SenecEntities;
using SenecEntitiesAdapter;
using SenecSource;
using Shared;
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
        public void Request_Fronius()
        {
            InitScope();

            var request = scope.Resolve<IGetPowerFlowRealtimeDataRequest>();
            request.Content = "";
            var response = request.Request<GetPowerFlowRealtimeDataResponse>(CancellationToken.None).RunWait();
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
            var energy = adapter.Convert(time.AsInteger.ToEquipmentLocalTime(scope.Resolve<IZoneProvider>()), response.ENERGY);
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
            cb.RegisterInstance<IFroniusSettings>(new FroniusSettings
            {
                IP = "192.168.0.101"
            }).SingleInstance();
            cb.RegisterInstance<ISenecSettings>(new SenecSettings
            {
                IP = "192.168.0.199"
            }).SingleInstance();
            cb.RegisterInstance<ILocalContextConfiguration>(new LocalContextConfiguration(
                accountEndPoint: "https://....documents.azure.com:443/",
                accountKey: "...",
                defaultContainer: "SenecDev",
                databaseName: "ToDoList"));
            cb.RegisterInstance<IZoneProvider>(new ZoneProvider("Australia/Perth"));
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
            InitScope();

            var now = DateTimeOffset.Now.ToEquipmentLocalTime(scope.Resolve<IZoneProvider>());
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

        [TestMethod]
        public void CloudCreate()
        {
            InitScope(a =>
                a.Item1.RegisterModule(new Repository.Cosmos.AutofacModule(a.Item2.Object))
            );
            var repo = scope.Resolve<IContext>();
            var obj = JsonConvert.DeserializeObject<Entities.EnergySummary>(@"{
  ""IntervalStartIncluded"": ""2020-09-14T20:38:04+00:00"",
  ""IntervalEndExcluded"": ""2020-09-14T20:39:00+00:00"",
  ""Failures"": 0,
  ""GridExportWatts"": {
    ""Minimum"": 0.0,
    ""Maximum"": 49.62,
    ""Median"": 18.16,
    ""Average"": 17.811666666666666666666666667,
    ""IsValid"": true
  },
  ""GridExportWattEnergy"": 857.48,
  ""GridImportWatts"": {
    ""Minimum"": 0.0,
    ""Maximum"": 10.86,
    ""Median"": 0.0,
    ""Average"": 0.3364583333333333333333333333,
    ""IsValid"": true
  },
  ""GridImportWattEnergy"": 16.65,
  ""ConsumptionWatts"": {
    ""Minimum"": 567.15,
    ""Maximum"": 607.96,
    ""Median"": 583.78,
    ""Average"": 583.595625,
    ""IsValid"": true
  },
  ""ConsumptionWattEnergy"": 29197.95,
  ""SolarPowerGenerationWatts"": {
    ""Minimum"": 0.0,
    ""Maximum"": 0.0,
    ""Median"": 0.0,
    ""Average"": 0.0,
    ""IsValid"": true
  },
  ""SolarPowerGenerationWattEnergy"": 0.0,
  ""BatteryChargeWatts"": {
    ""Minimum"": 0.0,
    ""Maximum"": 0.0,
    ""Median"": 0.0,
    ""Average"": 0.0,
    ""IsValid"": true
  },
  ""BatteryChargeWattEnergy"": 0.0,
  ""BatteryDischargeWatts"": {
    ""Minimum"": 593.69,
    ""Maximum"": 623.62,
    ""Median"": 598.68,
    ""Average"": 601.06979166666666666666666667,
    ""IsValid"": true
  },
  ""BatteryDischargeWattEnergy"": 30038.73,
  ""BatteryPercentageFull"": {
    ""Minimum"": 86.2,
    ""Maximum"": 86.2,
    ""Median"": 86.2,
    ""Average"": 86.2,
    ""IsValid"": true
  },
  ""SecondsBatteryCharging"": 0,
  ""SecondsBatteryDischarging"": 48,
  ""SecondsWithoutData"": 12
}");
            var createItemFunc = repo.CreateItemAsync(obj);
            var result = createItemFunc(CancellationToken.None).RunWait();
        }

        [TestMethod]
        public void FetchEnergy()
        {
            InitScope(a =>
                a.Item1.RegisterModule(new ReadRepository.Cosmos.AutofacModule(a.Item2.Object))
            );
            var readRepo = scope.Resolve<IEnergySummaryDocumentReadRepository>();
            var items = readRepo.Fetch(new DateTime(2020, 09, 15)).RunWait();
            var item = readRepo.Get("2020-09-15T19:30:00+00:00", CancellationToken.None).RunWait();
        }

        [TestMethod]
        public void FetchPower()
        {
            InitScope(a =>
                a.Item1.RegisterModule(new ReadRepository.Cosmos.AutofacModule(a.Item2.Object))
            );
            var readRepo = scope.Resolve<IEnergySummaryDocumentReadRepository>();
            var item = readRepo.GetPowerMovement("2020-09-23T00:00:00+00:00", CancellationToken.None).RunWait();
        }

        [TestMethod]
        public void FetchPower1()
        {
            InitScope(a =>
                a.Item1.RegisterModule(new ReadRepository.Cosmos.AutofacModule(a.Item2.Object))
            );
            var readRepo = scope.Resolve<IEnergySummaryDocumentReadRepository>();
            var item = readRepo.FetchPowerMovements(new DateTime(2020, 9, 23)).RunWait();
        }
    }

}
