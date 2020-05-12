using Autofac;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using ReadRepository;
using ReadRepository.Cosmos;
using Repository;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadRepository.Cosmos
{
    public class AutofacModule : Module
    {
        public AutofacModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<ILocalContextConfiguration>();
                return new CosmosClient(
                    configuration.AccountEndPoint,
                    configuration.AccountKey);
            }).AsSelf().SingleInstance();
            builder.Register(context =>
            {
                var configuration = context.Resolve<ILocalContextConfiguration>();
                var client = context.Resolve<CosmosClient>();
                var db = client.GetDatabase(configuration.DatabaseName);
                var container = db.GetContainer(configuration.DefaultContainer);
                return new ReadContainer(container) as IReadContext;
            }).InstancePerLifetimeScope();
            builder.RegisterType<VoltageSummaryDocumentReadRepository>().AsImplementedInterfaces();
        }
    }
}
