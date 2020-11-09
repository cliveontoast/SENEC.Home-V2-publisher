//using Autofac;
//using Domain;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using ReadRepository.Cosmos;
//using ReadRepository.ReadModel;
//using Repository;
//using Repository.Cosmos.Repositories;
//using Repository.Model;
//using SenecSourceWebAppTest;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Test
//{
//    [TestClass]
//    public class EnergyRepoStoreTests
//    {
//        public ILifetimeScope scope = Mock.Of<ILifetimeScope>();

//        public void InitScope(Action<(ContainerBuilder, Mock<IConfiguration>)>? extras = null)
//        {
//            var cb = UnitTest1.Builder();
//            extras?.Invoke(cb);
//            var container = cb.cb.Build();
//            scope = container.BeginLifetimeScope();
//        }

//        [TestCleanup]
//        public void Cleanup()
//        {
//            scope?.Dispose();
//        }


//        [TestMethod]
//        public void Handle_NotYetPersisted_FailsPersist()
//        {
//            InitScope(a =>
//            {
//                a.Item1.RegisterMock<IEnergySummaryDocumentReadRepository>();
//                a.Item1.RegisterMock<IEnergySummaryRepository>();
//                a.Item1.RegisterMock<IRepoConfig>();
//            });
//            var obj = new Entities.EnergySummary(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
//            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
//            mockRepo
//                .Setup(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(true));

//            var handler = scope.Resolve<INotificationHandler<Entities.EnergySummary>>();
//            Assert.AreEqual(typeof(EnergySummaryRepoStore), handler.GetType());

//            handler.Handle(obj, CancellationToken.None).RunWait();

//            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), CancellationToken.None), Times.Once());
//        }


//        [TestMethod]
//        public void Handle_NotYetPersisted_SuccessPersist()
//        {
//            InitScope(a =>
//            {
//                a.Item1.RegisterMock<IEnergySummaryDocumentReadRepository>();
//                a.Item1.RegisterMock<IEnergySummaryRepository>();
//                a.Item1.RegisterMock<IRepoConfig>();
//            });
//            var obj = new Entities.EnergySummary(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
//            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
//            mockRepo
//                .Setup(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(HttpStatusCode.OK));

//            int readRepo = 0;
//            var mockReadRepo = scope.Resolve<Mock<IEnergySummaryDocumentReadRepository>>()
//                .Setup(a => a.Get(obj.GetKey(), CancellationToken.None))
//                .Returns(() =>
//                {
//                    readRepo++;
//                    return Task.FromResult(readRepo == 5
//                        ? new EnergySummaryReadModel(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default)
//                        : null);
//                });


//            var handler = scope.Resolve<INotificationHandler<Entities.EnergySummary>>();
//            Assert.AreEqual(typeof(EnergySummaryRepoStore), handler.GetType());

//            handler.Handle(obj, CancellationToken.None).RunWait();

//            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), CancellationToken.None), Times.Once());
//        }

//        [TestMethod]
//        public void Handle_IstPersisted_IsNotPersisted()
//        {
//            InitScope(a =>
//            {
//                a.Item1.RegisterMock<IEnergySummaryDocumentReadRepository>();
//                a.Item1.RegisterMock<IEnergySummaryRepository>();
//            });
//            var obj = new Entities.EnergySummary(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
//            var mockRepo = scope.Resolve<Mock<IEnergySummaryRepository>>();
//            mockRepo
//                .Setup(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(HttpStatusCode.OK));
//            var mockReadRepo = scope.Resolve<Mock<IEnergySummaryDocumentReadRepository>>()
//                .Setup(a => a.Get(obj.GetKey(), CancellationToken.None))
//                .Returns(Task.FromResult(new EnergySummaryReadModel(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default)));

//            var handler = scope.Resolve<INotificationHandler<Entities.EnergySummary>>();
//            Assert.AreEqual(typeof(EnergySummaryRepoStore), handler.GetType());

//            handler.Handle(obj, CancellationToken.None).RunWait();

//            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.EnergySummary>(), CancellationToken.None), Times.Never());
//        }
//    }
//}
