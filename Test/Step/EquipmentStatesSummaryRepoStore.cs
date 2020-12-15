using Autofac;
using Domain;
using Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository;
using Repository.Cosmos.Repositories;
using Repository.Model;
using SenecSourceWebAppTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Test.Step
{
    [Binding]
    public class EquipmentStatesSummaryRepoStoreStep
    {
        // TODO wire up dependency injection
        public ILifetimeScope scope = Mock.Of<ILifetimeScope>();

        public void InitScope(Action<(ContainerBuilder, Mock<IConfiguration>)>? extras = null)
        {
            var cb = UnitTest1.Builder();
            extras?.Invoke(cb);
            var container = cb.cb.Build();
            scope = container.BeginLifetimeScope();
        }


        private EquipmentStatesSummaryScenarioContext _scenarioContext;

        public EquipmentStatesSummaryRepoStoreStep()
        {
            _scenarioContext = new EquipmentStatesSummaryScenarioContext();
        }

        [Given(@"equipment states dependency injection is a thing")]
        public void GivenDependencyInjectionIsAThing()
        {
            InitScope((a) =>
            {
                a.Item1.RegisterMock<IEquipmentStatesSummaryDocumentReadRepository>();
                a.Item1.RegisterMock<IEquipmentStatesSummaryRepository>();
                a.Item1.RegisterMock<IRepoConfig>();
            });
        }


        [Given(@"there is a equipment states summary for (.*)")]
        public void GivenThereIsAEquipmentStatesSummaryFor(DateTime p0)
        {
            var obj = new EquipmentStatesSummary(default, default, Enumerable.Empty<EquipmentStateStatistic>(), default);
            _scenarioContext.Summary = obj;
        }

        [Given(@"the equipment states summary has not been persisted")]
        public void GivenTheEquipmentStatesSummaryHasNotBeenPersisted()
        {
            var obj = new EquipmentStatesSummaryReadModel(default, default, "", default, new List<EquipmentStateStatistic>(), default);
            var mockReadRepo = scope.Resolve<Mock<IEquipmentStatesSummaryDocumentReadRepository>>();
            Func<Task<EquipmentStatesSummaryReadModel?>> FakeGetReadModel = () =>
            {
                _scenarioContext.ReadRepoCount++;
                return Task.FromResult(_scenarioContext.ReadRepoCountResults.Contains(_scenarioContext.ReadRepoCount)
                    ? obj
                    : null);
            };

            mockReadRepo
                .Setup(a => a.Get(_scenarioContext.Summary.GetKey(), CancellationToken.None))
                .Returns(FakeGetReadModel);
        }

        [Given(@"equipment states persistance is available")]
        public void GivenPersistanceIsAvailable()
        {
            _scenarioContext.ReadRepoCountResults.Add(5);
            var mockRepo = scope.Resolve<Mock<IEquipmentStatesSummaryRepository>>();
            mockRepo
                .Setup(a => a.AddAsync(It.IsAny<EquipmentStatesSummary>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
        }

        [Given(@"equipment states persistance is available only on a retry")]
        public void GivenPersistanceIsAvailableOnRetry()
        {
            _scenarioContext.ReadRepoCountResults.Add(10);
            int count = 0;
            var mockRepo = scope.Resolve<Mock<IEquipmentStatesSummaryRepository>>();
            mockRepo
                .Setup(a => a.AddAsync(It.IsAny<EquipmentStatesSummary>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count == 2 ? true : false);
                });
        }


        [When(@"the equipment states summary is persisted")]
        public void WhenTheEquipmentStatesSummaryIsPersisted()
        {
            var handler = scope.Resolve<INotificationHandler<EquipmentStatesSummary>>();
            Assert.AreEqual(typeof(EquipmentStatesSummaryRepoStore), handler.GetType());

            handler.Handle(_scenarioContext.Summary, CancellationToken.None).RunWait();
        }

        [Then(@"the equipment states summary should have been persisted")]
        public void ThenTheEquipmentStatesSummaryShouldHaveBeenPersisted()
        {
            var mockRepo = scope.Resolve<Mock<IEquipmentStatesSummaryRepository>>();
            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.EquipmentStatesSummary>(), CancellationToken.None), Times.Once());
        }

        [Then(@"the equipment states summary should have been persisted the second time")]
        public void ThenTheEquipmentStatesSummaryShouldHaveBeenPersistedTheSecondTime()
        {
            var mockRepo = scope.Resolve<Mock<IEquipmentStatesSummaryRepository>>();
            mockRepo.Verify(a => a.AddAsync(It.IsAny<EquipmentStatesSummary>(), CancellationToken.None), Times.Exactly(2));
        }

    }

    internal class EquipmentStatesSummaryScenarioContext
    {
        public EquipmentStatesSummary Summary { get; internal set; } = null!;
        public int ReadRepoCount { get; set; }
        public List<int> ReadRepoCountResults { get; set; } = new List<int>();
    }
}
