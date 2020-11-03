using Autofac;
using Domain;
using Entities;
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
                a.Item1.RegisterMock<IRepoConfig>();
            });
            scope.Resolve<Mock<IRepoConfig>>().Setup(a => a.Testing).Returns(true);
            var mediator = scope.Resolve<IMediator>();
            var voltageSummaryCollector = scope.Resolve<Mock<INotificationHandler<VoltageSummary>>>();
            var energyCollector = scope.Resolve<Mock<INotificationHandler<EnergySummary>>>();

            voltageSummaryCollector.Setup(a => a.Handle(It.IsAny<VoltageSummary>(), It.IsAny<CancellationToken>()))
                .Callback<VoltageSummary, CancellationToken>((a, b) =>
                {
                    var txt = JsonConvert.SerializeObject(a, Formatting.Indented);
                    //File.WriteAllText(@"..\..\..\Resources\LongRun\lalaVoltageResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json", txt);
                    var expected = File.ReadAllText(@"..\..\..\Resources\LongRun\lalaVoltageResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json");
                    Assert.AreEqual(expected, txt);
                });

            energyCollector.Setup(a => a.Handle(It.IsAny<EnergySummary>(), It.IsAny<CancellationToken>()))
                .Callback<EnergySummary, CancellationToken>((a, b) =>
                {
                    var txt = JsonConvert.SerializeObject(a, Formatting.Indented);
                    //File.WriteAllText(@"..\..\..\Resources\LongRun\lalaEnergyResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json", txt);
                    var expected = File.ReadAllText(@"..\..\..\Resources\LongRun\lalaEnergyResults\" + a.IntervalStartIncluded.ToUnixTimeSeconds() + ".json");
                    Assert.AreEqual(expected, txt);
                });

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
            }
            scope.RunWait(scope.Resolve<SenecGridMeterSummaryCommand>());
            scope.RunWait(scope.Resolve<SenecEnergySummaryCommand>());
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
}
