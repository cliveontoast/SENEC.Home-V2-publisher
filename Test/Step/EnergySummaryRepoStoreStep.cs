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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Test.Step
{
    [Binding]
    public class EnergySummaryRepoStoreStep
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


        private EnergySummaryScenarioContext _scenarioContext;

        public EnergySummaryRepoStoreStep()
        {
            _scenarioContext = new EnergySummaryScenarioContext();
        }

        [Given(@"energy dependency injection is a thing")]
        public void GivenDependencyInjectionIsAThing()
        {
            InitScope((a) =>
            {
                a.Item1.RegisterMock<IEnergySummaryDocumentReadRepository>();
                a.Item1.RegisterMock<IEnergySummaryRepository>();
                a.Item1.RegisterMock<IRepoConfig>();
            });
        }


        [Given(@"there is a energy summary for (.*)")]
        public void GivenThereIsAEnergySummaryFor(DateTime p0)
        {
            var obj = new EnergySummary(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
            _scenarioContext.Summary = obj;
        }

        [Given(@"the energy summary has not been persisted")]
        public void GivenTheEnergySummaryHasNotBeenPersisted()
        {
            var obj = new EnergySummaryReadModel(default, default, "", default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
            var mockReadRepo = scope.Resolve<Mock<IEnergySummaryDocumentReadRepository>>();
            Func<Task<EnergySummaryReadModel?>> FakeGetReadModel = () =>
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

        [Given(@"energy persistance is available")]
        public void GivenPersistanceIsAvailable()
        {
            _scenarioContext.ReadRepoCountResults.Add(5);
            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
            mockRepo
                .Setup(a => a.AddAsync(It.IsAny<EnergySummary>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
        }

        [Given(@"energy persistance is available only on a retry")]
        public void GivenPersistanceIsAvailableOnRetry()
        {
            _scenarioContext.ReadRepoCountResults.Add(10);
            int count = 0;
            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
            mockRepo
                .Setup(a => a.AddAsync(It.IsAny<EnergySummary>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count == 2 ? true : false);
                });
        }


        [When(@"the energy summary is persisted")]
        public void WhenTheEnergySummaryIsPersisted()
        {
            var handler = scope.Resolve<INotificationHandler<EnergySummary>>();
            Assert.AreEqual(typeof(EnergySummaryRepoStore), handler.GetType());

            handler.Handle(_scenarioContext.Summary, CancellationToken.None).RunWait();
        }

        [Then(@"the energy summary should have been persisted")]
        public void ThenTheEnergySummaryShouldHaveBeenPersisted()
        {
            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), CancellationToken.None), Times.Once());
        }

        [Then(@"the energy summary should have been persisted the second time")]
        public void ThenTheEnergySummaryShouldHaveBeenPersistedTheSecondTime()
        {
            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
            mockRepo.Verify(a => a.AddAsync(It.IsAny<EnergySummary>(), CancellationToken.None), Times.Exactly(2));
        }

    }

    internal class EnergySummaryScenarioContext
    {
        public EnergySummary Summary { get; internal set; } = null!;
        public int ReadRepoCount { get; set; }
        public List<int> ReadRepoCountResults { get; set; } = new List<int>();
    }
}
