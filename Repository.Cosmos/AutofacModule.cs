using Autofac;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Repository;
using Repository.Cosmos;
using Repository.Cosmos.Repositories;

namespace Repository.Cosmos
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
                return new WriteContext(container) as IContext;
            }).InstancePerLifetimeScope();
            builder.RegisterType<EnergySummaryRepository>().AsImplementedInterfaces();
        }
    }
}
