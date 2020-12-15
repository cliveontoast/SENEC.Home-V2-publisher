using Autofac;
using Domain;
using Entities;
using FroniusSource;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using SenecEntities;
using SenecSource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SenecSourceWebAppTest
{
    [TestClass]
    public class LongRun
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
          
        public static bool finished = false;
        [TestMethod]
        public void FetchEnergy()
        {
            InitScope(a =>
            {
                //a.Item1.RegisterModule(new ReadRepository.Cosmos.AutofacModule(a.Item2.Object))
                a.Item1.RegisterInstance<ILalaRequest>(new FileLala(@"..\..\..\Resources\LongRun\lala"));
                a.Item1.RegisterInstance<IGetPowerFlowRealtimeDataRequest>(new FileFlow(@"..\..\..\Resources\LongRun\powerflow"));
                a.Item1.RegisterInstance<ISenecVoltCompressConfig>(new SenecCompressConfig
                {
                    MinutesPerSummary = 5,
                    PersistedVersion = 0,
                });
                a.Item1.RegisterInstance<ISenecEnergyCompressConfig>(new SenecCompressConfig
                {
                    MinutesPerSummary = 5,
                    PersistedVersion = 0,
                });
                a.Item1.RegisterMock<INotificationHandler<VoltageSummary>>();
                a.Item1.RegisterMock<INotificationHandler<EnergySummary>>();
                a.Item1.RegisterMock<INotificationHandler<EquipmentStatesSummary>>();
                a.Item1.RegisterMock<IRepoConfig>();
            });
            scope.Resolve<Mock<IRepoConfig>>().Setup(a => a.Testing).Returns(true);
            var mediator = scope.Resolve<IMediator>();
            var voltageSummaryCollector = scope.Resolve<Mock<INotificationHandler<VoltageSummary>>>();
            var energyCollector = scope.Resolve<Mock<INotificationHandler<EnergySummary>>>();
            var statesCollector = scope.Resolve<Mock<INotificationHandler<EquipmentStatesSummary>>>();

            int voltageSummaryCollectorCallbackCount = 0;
            const string VOLTAGE_RESULTS = @"..\..\..\Resources\LongRun\lalaVoltageResults\";
            string voltExpected = "";
            string voltActual = "";
            voltageSummaryCollector.Setup(a => a.Handle(It.IsAny<VoltageSummary>(), It.IsAny<CancellationToken>()))
                .Callback<VoltageSummary, CancellationToken>((a, b) =>
                {
                    voltActual = JsonConvert.SerializeObject(a, Formatting.Indented);
                    //File.WriteAllText(@"..\..\..\Resources\LongRun\lalaVoltageResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json", voltActual);
                    voltExpected = File.ReadAllText(VOLTAGE_RESULTS + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json");
                    voltageSummaryCollectorCallbackCount++;
                });

            int energyCollectorCallbackCount = 0;
            const string  ENERGY_RESULTS= @"..\..\..\Resources\LongRun\lalaEnergyResults\";
            string energyExpected = "";
            string energyActual = "";
            energyCollector.Setup(a => a.Handle(It.IsAny<EnergySummary>(), It.IsAny<CancellationToken>()))
                .Callback<EnergySummary, CancellationToken>((a, b) =>
                {
                    energyActual = JsonConvert.SerializeObject(a, Formatting.Indented);
                    //File.WriteAllText(@"..\..\..\Resources\LongRun\lalaEnergyResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json", energyActual);
                    energyExpected = File.ReadAllText(ENERGY_RESULTS + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json");
                    energyCollectorCallbackCount++;
                });

            int statesCollectorCallbackCount = 0;
            const string  STATES_RESULTS= @"..\..\..\Resources\LongRun\lalaStatesResults\";
            string statesExpected = "";
            string statesActual = "";
            statesCollector.Setup(a => a.Handle(It.IsAny<EquipmentStatesSummary>(), It.IsAny<CancellationToken>()))
                .Callback<EquipmentStatesSummary, CancellationToken>((a, b) =>
                {
                    statesActual = JsonConvert.SerializeObject(a, Formatting.Indented);
                    //File.WriteAllText(@"..\..\..\Resources\LongRun\lalaStatesResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json", statesActual);
                    statesExpected = File.ReadAllText(STATES_RESULTS + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json");
                    statesCollectorCallbackCount++;
                });

            while (!finished)
            {
                scope.RunWaitResponse<FroniusPollCommand, TimeSpan?>(scope.Resolve<FroniusPollCommand>());
            }
            finished = false;
            int i = 0;
            while (!finished)
            {
                i++;
                scope.RunWait(scope.Resolve<SenecPollCommand>());
                if (i % 10 == 0)
                {
                    scope.RunWait(scope.Resolve<SenecGridMeterSummaryCommand>());
                    scope.RunWait(scope.Resolve<SenecEnergySummaryCommand>());
                }
                Assert.AreEqual(energyExpected, energyActual);
                Assert.AreEqual(voltExpected, voltActual);
                Assert.AreEqual(statesExpected, statesActual);
            }
            scope.RunWait(scope.Resolve<SenecGridMeterSummaryCommand>());
            scope.RunWait(scope.Resolve<SenecEnergySummaryCommand>());
            Assert.AreEqual(energyExpected, energyActual);
            Assert.AreEqual(voltExpected, voltActual);
            Assert.AreEqual(statesExpected, statesActual);

            Assert.AreEqual(Directory.GetFiles(ENERGY_RESULTS).Length, energyCollectorCallbackCount);
            Assert.AreEqual(Directory.GetFiles(VOLTAGE_RESULTS).Length, voltageSummaryCollectorCallbackCount);
            Assert.AreEqual(Directory.GetFiles(STATES_RESULTS).Length, statesCollectorCallbackCount);
        }
    }

    public class FileLala : ILalaRequest
    {
        private readonly string _directory;
        private readonly IEnumerator<string> _enum;

        public FileLala(string directory)
        {
            _directory = directory;
            _enum = System.IO.Directory.EnumerateFiles(_directory).GetEnumerator();
        }
        public string? Content { get; set; }

        public async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            LongRun.finished = !_enum.MoveNext();
            if (LongRun.finished)
                return await Task.FromResult(Activator.CreateInstance<TResponse>());

            var content = await File.ReadAllTextAsync(_enum.Current);
            var response = JsonConvert.DeserializeObject<TResponse>(value: content, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return response!;
        }
    }

    public class FileFlow : IGetPowerFlowRealtimeDataRequest
    {
        private readonly string _directory;
        private readonly IEnumerator<string> _enum;

        public FileFlow(string directory)
        {
            _directory = directory;
            _enum = System.IO.Directory.EnumerateFiles(_directory).GetEnumerator();
        }
        public string? Content { get; set; }
        public TimeSpan? Timeout { get; set; }

        public async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            LongRun.finished = !_enum.MoveNext();
            if (LongRun.finished)
                return await Task.FromResult(Activator.CreateInstance<TResponse>());

            var content = await File.ReadAllTextAsync(_enum.Current);
            var response = JsonConvert.DeserializeObject<TResponse>(value: content, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return response!;
        }
    }
}
