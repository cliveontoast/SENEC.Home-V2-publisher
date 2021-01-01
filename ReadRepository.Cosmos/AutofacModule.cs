using Autofac;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Repository;

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
            builder.RegisterType<EnergySummaryDocumentReadRepository>().AsImplementedInterfaces();
            builder.RegisterType<BatteryInverterTemperatureSummaryDocumentReadRepository>().AsImplementedInterfaces();
            builder.RegisterType<EquipmentStatesSummaryDocumentReadRepository>().AsImplementedInterfaces();
            builder.RegisterType<PublisherDocumentReadRepository>().AsImplementedInterfaces();
        }
    }
}
