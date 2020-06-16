using Autofac;
using Domain.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Repository;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cloud.Domain
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

            Shared(builder);
        }

        private void Shared(ContainerBuilder builder)
        {
            builder.RegisterType<TimeProvider>().AsImplementedInterfaces();
            builder.RegisterType<ApplicationVersion>().AsImplementedInterfaces().SingleInstance();
            builder.Register((context) =>
            {
                var result = Configuration.GetSection("CosmosDB").Get<LocalContextConfiguration>();
                return result as ILocalContextConfiguration;
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
            var assembly = typeof(DailyVoltageSummaryCommand).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => mediatrInterfaces.All(m => !t.IsClosedTypeOf(m)))
                .AsImplementedInterfaces();

            builder.RegisterType<DailyVoltageSummaryCommand>().AsSelf();
        }
    }
}
