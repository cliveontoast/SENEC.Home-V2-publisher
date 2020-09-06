using Autofac;
using Domain.Commands;
using Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Cloud.Test
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

        [TestInitialize]
        public void Init()
        {
            var container = TestSetup.GetContainerBuilder().Build();
            scope = container.BeginLifetimeScope();
        }

        [TestMethod]
        public void FetchVoltageSummary()
        {
            var results = scope.RunSync<DailyVoltageSummaryCommand, VoltageSummaryDaily>(new DailyVoltageSummaryCommand { Date = new DateTime(2020, 05, 14) });

        }
    }
}
