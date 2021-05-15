using Autofac;
using Domain;
using Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;
using TeslaPowerwallSource;

namespace SenecSourceWebAppTest
{
    [TestClass]
    public class LongRunTesla
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
                var s = new Mock<IConfigurationSection>();
                s.Setup(a => a.Value).Returns("");
                var v = new Mock<IConfigurationSection>();
                v.Setup(a => a.Value).Returns("available");
                a.Item2.Setup(a => a.GetSection("SenecIP")).Returns(s.Object);
                a.Item2.Setup(a => a.GetSection("FroniusIP")).Returns(s.Object);
                a.Item2.Setup(a => a.GetSection("TeslaPowerwall2IP")).Returns(v.Object);
                a.Item1.RegisterInstance<IApiRequest>(new FileRequest(@"..\..\..\Tesla-2021-04-03\"));
            });
            //scope.Resolve<Mock<IRepoConfig>>().Setup(a => a.Testing).Returns(true);
            var mediator = scope.Resolve<IMediator>();

            //while (!finished)
            //{
            //    scope.RunWaitResponse<TeslaEnergySummaryCommand, Unit>(scope.Resolve<TeslaEnergySummaryCommand>());
            //}
            //finished = false;
            int i = 0;
            while (!finished)
            {
                i++;
                scope.RunWait(scope.Resolve<TeslaPowerwall2PollCommand>());
                if (i % 10 == 0)
                {
                    scope.RunWait(scope.Resolve<TeslaEnergySummaryCommand>());
                }
            }
            scope.RunWait(scope.Resolve<TeslaEnergySummaryCommand>());
        }
    }

    public class FileRequest : IApiRequest
    {
        private readonly string _directory;
        private readonly IEnumerator<string> _enumMeter;
        private readonly IEnumerator<string> _enumEnergy;

        public FileRequest(string directory)
        {
            _directory = directory;
            _enumMeter = Directory.EnumerateFiles(Path.Combine(_directory, "Meters")).OrderBy(a => a).GetEnumerator();
            _enumEnergy = Directory.EnumerateFiles(Path.Combine(_directory, "Energy")).OrderBy(a => a).GetEnumerator();
        }
        public string? Content { get; set; }

        public void Destroy(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetersAggregates> GetMetersAggregatesAsync(CancellationToken token)
        {
            LongRunTesla.finished = !_enumMeter.MoveNext();
            if (LongRun.finished)
                return await Task.FromResult(Activator.CreateInstance<MetersAggregates>());

            var content = await File.ReadAllTextAsync(_enumMeter.Current);
            var response = JsonConvert.DeserializeObject<MetersAggregates>(value: content, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return response!;
        }

        public async Task<StateOfEnergy> GetStateOfEnergyAsync(CancellationToken token)
        {
            var finished = !_enumEnergy.MoveNext();
            if (finished)
                return await Task.FromResult(Activator.CreateInstance<StateOfEnergy>());

            var content = await File.ReadAllTextAsync(_enumEnergy.Current);
            var response = JsonConvert.DeserializeObject<StateOfEnergy>(value: content, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return response!;
        }
    }
}
