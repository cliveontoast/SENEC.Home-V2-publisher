using Autofac;
using Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using Repository;
using Repository.Model;
using SenecSourceWebAppTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Test.Step
{
    [Binding]
    public class VoltageSummaryRepoStoreStep
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


        private VoltageSummaryScenarioContext _scenarioContext;

        public VoltageSummaryRepoStoreStep()
        {
            _scenarioContext = new VoltageSummaryScenarioContext();
        }

        [Given(@"dependency injection is a thing")]
        public void GivenDependencyInjectionIsAThing()
        {
            InitScope((a) =>
            {
                a.Item1.RegisterMock<IVoltageSummaryDocumentReadRepository>();
                a.Item1.RegisterMock<IVoltageSummaryRepository>();
                a.Item1.RegisterMock<IRepoConfig>();
            });
        }


        [Given(@"there is a voltage summary for (.*)")]
        public void GivenThereIsAVoltageSummaryFor(DateTime p0)
        {
            _scenarioContext.Summary = new VoltageSummary();
        }

        [Given(@"the voltage summary has not been persisted")]
        public void GivenTheVoltageSummaryHasNotBeenPersisted()
        {
            var mockReadRepo = scope.Resolve<Mock<IVoltageSummaryDocumentReadRepository>>();
            Func<Task<VoltageSummaryReadModel?>> FakeGetReadModel = () =>
            {
                _scenarioContext.ReadRepoCount++;
                return Task.FromResult(_scenarioContext.ReadRepoCountResults.Contains(_scenarioContext.ReadRepoCount)
                    ? new VoltageSummaryReadModel(default, default, "", default, default, default, default)
                    : null);
            };

            mockReadRepo
                .Setup(a => a.Get(_scenarioContext.Summary.GetKey(), CancellationToken.None))
                .Returns(FakeGetReadModel);
        }

        [Given(@"persistance is available")]
        public void GivenPersistanceIsAvailable()
        {
            _scenarioContext.ReadRepoCountResults.Add(5);
            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
            mockRepo
                .Setup(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
        }

        [Given(@"persistance is available only on a retry")]
        public void GivenPersistanceIsAvailableOnRetry()
        {
            _scenarioContext.ReadRepoCountResults.Add(10);
            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
            mockRepo
                .Setup(a => a.AddAsync(It.IsAny<VoltageSummary>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
        }


        [When(@"the voltage summary is persisted")]
        public void WhenTheVoltageSummaryIsPersisted()
        {
            var handler = scope.Resolve<INotificationHandler<Entities.VoltageSummary>>();
            Assert.AreEqual(typeof(VoltageSummaryRepoStore), handler.GetType());

            handler.Handle(_scenarioContext.Summary, CancellationToken.None).RunWait();
        }

        [Then(@"the voltage summary should have been persisted")]
        public void ThenTheVoltageSummaryShouldHaveBeenPersisted()
        {
            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), CancellationToken.None), Times.Once());
        }

        [Then(@"the voltage summary should have been persisted the second time")]
        public void ThenTheVoltageSummaryShouldHaveBeenPersistedTheSecondTime()
        {
            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
            mockRepo.Verify(a => a.AddAsync(It.IsAny<VoltageSummary>(), CancellationToken.None), Times.Once());
        }

    }

    internal class VoltageSummaryScenarioContext
    {
        public VoltageSummary Summary { get; internal set; } = null!;
        public int ReadRepoCount { get; set; }
        public List<int> ReadRepoCountResults { get; set; } = new List<int>();
    }
}
