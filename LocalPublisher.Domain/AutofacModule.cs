using Autofac;
using FroniusSource;
using MediatR;
using Microsoft.Extensions.Configuration;
using Repository;
using SenecEntitiesAdapter;
using SenecSource;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using TeslaPowerwallSource;

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
            Repository(builder);

            SenecSource(builder);
            FroniusSource(builder);
            TeslaSource(builder);

            Shared(builder);
            builder.RegisterInstance(new ZoneProvider(Configuration.GetValue<string>("Timezone")) as IZoneProvider).SingleInstance();
            builder.RegisterInstance(new RepoConfig() as IRepoConfig);
        }

        private bool IsSenecAvailable => !string.IsNullOrWhiteSpace(Configuration.GetValue<string?>("SenecIP"));
        private bool IsFroniusAvailable => !string.IsNullOrWhiteSpace(Configuration.GetValue<string?>("FroniusIP"));
        private bool IsTeslaPowerwall2Available => !string.IsNullOrWhiteSpace(Configuration.GetValue<string?>("TeslaPowerwall2IP"));


        private void Repository(ContainerBuilder builder)
        {
            builder.RegisterType<VoltageSummaryRepository>().As<IVoltageSummaryRepository>();
            builder.RegisterType<LocalContext>().As<ILocalContext>();
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

        private void FroniusSource(ContainerBuilder builder)
        {
            if (!IsFroniusAvailable) return;

            var assembly = typeof(GetPowerFlowRealtimeDataRequest).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(a => a != typeof(FroniusSettings))
                .AsImplementedInterfaces();
            builder.Register((context) =>
            {
                return new FroniusSettings
                {
                    IP = Configuration.GetValue<string?>("FroniusIP")
                } as IFroniusSettings;
            }).SingleInstance();
        }

        private void SenecSource(ContainerBuilder builder)
        {
            if (!IsSenecAvailable) return;

            var assembly = typeof(ILalaRequest).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(a => a != typeof(SenecSettings))
                .AsImplementedInterfaces();
            builder.Register((context) =>
            {
                return new SenecSettings
                {
                    IP = Configuration.GetValue<string?>("SenecIP")
                } as ISenecSettings;
            }).SingleInstance();

            builder.RegisterAssemblyTypes(typeof(IAdapter).Assembly).AsImplementedInterfaces();
        }

        private void TeslaSource(ContainerBuilder builder)
        {
            if (!IsTeslaPowerwall2Available) return;

            var assembly = typeof(ITeslaPowerwallSettings).Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(a => a != typeof(TeslaPowerwallSettings))
                // temporary
                //.Where(a => a != typeof(ApiRequest))
                .AsImplementedInterfaces();
            builder.Register((context) =>
            {
                return new TeslaPowerwallSettings
                {
                    IP = Configuration.GetValue<string?>("TeslaPowerwall2IP"),
                    Password = Configuration.GetValue<string>("TeslaPowerwall2Password"),
                    CredentialCacheSeconds = Configuration.GetValue<double>("CredentialCacheSeconds"),
                } as ITeslaPowerwallSettings;
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

            var assembly = this.GetType().Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => mediatrInterfaces.All(m => !t.IsClosedTypeOf(m)))
                .AsImplementedInterfaces();

            Senec(builder);
            Fronius(builder);
            Tesla(builder);
        }

        private void Tesla(ContainerBuilder builder)
        {
            if (!IsTeslaPowerwall2Available) return;

            builder.Register((context) =>
            {
                var result = Configuration.GetSection("EnergySummary").Get<SenecCompressConfig>();
                return result as ITeslaEnergyCompressConfig;
            });
            builder.RegisterType<TeslaPowerwall2PollCommand>().AsSelf();
            builder.RegisterType<TeslaEnergySummaryCommand>().AsSelf();
        }

        private void Fronius(ContainerBuilder builder)
        {
            if (!IsFroniusAvailable) return;

            builder.RegisterType<FroniusPollCommand>().AsSelf();
            builder.RegisterType<FroniusClearCommand>().AsSelf();
        }

        private void Senec(ContainerBuilder builder)
        {
            if (!IsSenecAvailable) return;

            builder.Register((context) =>
            {
                var result = Configuration.GetSection("VoltSummary").Get<SenecCompressConfig>();
                return result as ISenecVoltCompressConfig;
            });
            builder.Register((context) =>
            {
                var result = Configuration.GetSection("EnergySummary").Get<SenecCompressConfig>();
                return result as ISenecEnergyCompressConfig;
            });
            builder.Register((context) =>
            {
                var result = Configuration.GetSection("BatteryInverterTemperatureSummary").Get<SenecCompressConfig>();
                return result as ISenecBatteryInverterTemperatureCompressConfig;
            });
            builder.RegisterType<SenecPollCommand>().AsSelf();
            builder.RegisterType<SenecGridMeterSummaryCommand>().AsSelf();
            builder.RegisterType<SenecEnergySummaryCommand>().AsSelf();
            builder.RegisterType<SenecBatteryInverterSummaryCommand>().AsSelf();
        }
    }
}
