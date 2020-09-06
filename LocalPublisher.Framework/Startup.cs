using Autofac;
using Domain;
using LazyCache;
using LazyCache.Providers;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

#nullable enable
namespace LocalPublisherMono
{
    public class Startup
    {
        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables("LP_")
                .Build();
        }

        private IConfiguration Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>();
            builder.AddMediatR(typeof(GridMeterCache).Assembly);
            builder.RegisterModule(new AutofacModule(Configuration));
            builder.RegisterModule(new ReadRepository.Cosmos.AutofacModule(Configuration));
            builder.RegisterInstance(BuildAppCache());
            builder.RegisterType<TimedHostedService>().AsSelf();
        }

        public IAppCache BuildAppCache()
        {
            return new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }
    }
}
#nullable disable