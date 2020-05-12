﻿using Autofac;
using MediatR;
using Microsoft.Extensions.Configuration;
using ReadRepository;
using ReadRepository.Repositories;
using Repository;
using SenecEntitiesAdapter;
using SenecSource;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain
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
            Repository(builder);
            ReadRepo(builder);

            SenecSource(builder);
            Shared(builder);
        }

        private void ReadRepo(ContainerBuilder builder)
        {
            builder.RegisterType<VoltageSummaryReadRepository>().As<IVoltageSummaryReadRepository>();
            builder.RegisterType<ReadContext>()
                .As<IReadContext>()
                .InstancePerLifetimeScope();
        }

        private void Repository(ContainerBuilder builder)
        {
            builder.RegisterType<VoltageSummaryRepository>().As<IVoltageSummaryRepository>();
            builder.RegisterType<LocalContext>()
                .As<ILocalContext>()
                .InstancePerLifetimeScope();
        }

        private void Shared(ContainerBuilder builder)
        {
            builder.RegisterType<TimeProvider>().AsImplementedInterfaces();
            builder.RegisterType<ApplicationVersion>().AsImplementedInterfaces().SingleInstance();
            builder.Register((context) =>
            {
                var result = Configuration.GetSection("CosmosDB").Get<LocalContextConfiguration>();
                return result as ILocalContextConfiguration;
            });
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
