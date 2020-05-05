using Autofac;
using Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Repository;
using SenecEntitiesAdapter;
using SenecSource;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalPublisherWebApp
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
            Domain(builder);

            builder.RegisterAssemblyTypes(typeof(IAdapter).Assembly).AsImplementedInterfaces();
            builder.RegisterType<VoltageSummaryRepository>().As<IVoltageSummaryRepository>();
            builder.RegisterType<LocalContext>()
                .As<ILocalContext>()
                .InstancePerLifetimeScope();
            builder.Register((context) =>
            {
                var result = Configuration.GetSection("CosmosDB").Get<LocalContextConfiguration>();
                return result as ILocalContextConfiguration;
            });

            SenecSource(builder);
            Shared(builder);
        }

        private void Shared(ContainerBuilder builder)
        {
            var assembly = typeof(ITimeProvider).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .AsImplementedInterfaces();
        }

        private void SenecSource(ContainerBuilder builder)
        {
            var assembly = typeof(ILalaRequest).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(a => a != typeof(SenecSettings))
                .AsImplementedInterfaces();
            builder.Register((context) =>
            {
                return new SenecSettings
                {
                    IP = Configuration.GetValue<string>("SenecIP")
                } as ISenecSettings;
            }).SingleInstance();
        }

        private void Domain(ContainerBuilder builder)
        {
            var mediatrInterfaces = new List<Type>
            {
                // mediatr registrations happen in the service.AddMediatr() call;
                typeof(IRequestHandler<,>),
                typeof(IRequestHandler<>),
                typeof(INotificationHandler<>),
            };
            var assembly = typeof(SenecPollCommand).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => mediatrInterfaces.All(m => !t.IsClosedTypeOf(m)))
                .AsImplementedInterfaces();

            builder.Register((context) =>
            {
                var result = Configuration.GetSection("VoltSummary").Get<SenecCompressConfig>();
                return result as ISenecCompressConfig;
            });
            builder.RegisterType<SenecPollCommand>().AsSelf();
            builder.RegisterType<SenecGridMeterSummaryCommand>().AsSelf();
        }
    }
}
