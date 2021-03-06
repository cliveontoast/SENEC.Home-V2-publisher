﻿//using Autofac;
//using Domain;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using ReadRepository.Cosmos;
//using ReadRepository.ReadModel;
//using Repository;
//using Repository.Model;
//using SenecSourceWebAppTest;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Test
//{
//    [TestClass]
//    public class VoltageRepoStoreTests
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
//                a.Item1.RegisterMock<IVoltageSummaryDocumentReadRepository>();
//                a.Item1.RegisterMock<IVoltageSummaryRepository>();
//                a.Item1.RegisterMock<IRepoConfig>();
//            });
//            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
//            mockRepo
//                .Setup(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(1));

//            var handler = scope.Resolve<INotificationHandler<Entities.VoltageSummary>>();
//            Assert.AreEqual(typeof(VoltageSummaryRepoStore), handler.GetType());

//            handler.Handle(new VoltageSummary(), CancellationToken.None).RunWait();

//            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), CancellationToken.None), Times.Once());
//        }


//        [TestMethod]
//        public void Handle_NotYetPersisted_SuccessPersist()
//        {
//            InitScope(a =>
//            {
//                a.Item1.RegisterMock<IVoltageSummaryDocumentReadRepository>();
//                a.Item1.RegisterMock<IVoltageSummaryRepository>();
//                a.Item1.RegisterMock<IRepoConfig>();
//            });
//            var obj = new VoltageSummary();
//            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
//            mockRepo
//                .Setup(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(1));

//            int readRepo = 0;
//            var mockReadRepo = scope.Resolve<Mock<IVoltageSummaryDocumentReadRepository>>()
//                .Setup(a => a.Get(obj.GetKey(), CancellationToken.None))
//                .Returns(() =>
//                {
//                    readRepo++;
//                    return Task.FromResult(readRepo == 5
//                        ? new VoltageSummaryReadModel(default, default, "", default, default, default, default)
//                        : null);
//                });


//            var handler = scope.Resolve<INotificationHandler<Entities.VoltageSummary>>();
//            Assert.AreEqual(typeof(VoltageSummaryRepoStore), handler.GetType());

//            handler.Handle(obj, CancellationToken.None).RunWait();

//            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), CancellationToken.None), Times.Once());
//        }

//        [TestMethod]
//        public void Handle_IstPersisted_IsNotPersisted()
//        {
//            InitScope(a =>
//            {
//                a.Item1.RegisterMock<IVoltageSummaryDocumentReadRepository>();
//                a.Item1.RegisterMock<IVoltageSummaryRepository>();
//            });
//            var obj = new VoltageSummary();
//            var mockRepo = scope.Resolve<Mock<IVoltageSummaryRepository>>();
//            mockRepo
//                .Setup(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(1));
//            var mockReadRepo = scope.Resolve<Mock<IVoltageSummaryDocumentReadRepository>>()
//                .Setup(a => a.Get(obj.GetKey(), CancellationToken.None))
//                .Returns(Task.FromResult(new VoltageSummaryReadModel(default, default, default, default, default, default, default)));

//            var handler = scope.Resolve<INotificationHandler<Entities.VoltageSummary>>();
//            Assert.AreEqual(typeof(VoltageSummaryRepoStore), handler.GetType());

//            handler.Handle(obj, CancellationToken.None).RunWait();

//            mockRepo.Verify(a => a.AddAsync(It.IsAny<Entities.VoltageSummary>(), CancellationToken.None), Times.Never());
//        }
//    }
//}
