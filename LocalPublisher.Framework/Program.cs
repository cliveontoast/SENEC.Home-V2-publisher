using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain;
using LazyCache;
using LazyCache.Providers;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LocalPublisherMono
{
    class Program
    {
        static IConfigurationRoot Configuration;
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("Starting up");

                IServiceCollection services = new ServiceCollection();
                var cb = BuildContainer(services);
                ConfigureServices(services);
                ConfigureContainer(cb);
                var container = cb.Build();
                using (var tokenSource = new CancellationTokenSource())
                using (var scope = container.BeginLifetimeScope())
                {
                    var service = scope.Resolve<TimedHostedService>();
                    var hostTask = service.StartAsync(tokenSource.Token);
                    var cancelTask = Task.Factory.StartNew(() =>
                    {
                        Log.Information("Press ENTER to quit");
                        Console.ReadLine();
                        tokenSource.Cancel();
                    });
                    Task.WaitAll(hostTask, cancelTask);
                    if (hostTask.Exception != null)
                        Log.Fatal(hostTask.Exception, "Application failed.");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static ContainerBuilder BuildContainer(IServiceCollection services)
        {
            var factory = new AutofacServiceProviderFactory();
            var cb = factory.CreateBuilder(services);
            return cb;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables("LP_")
                .Build();
            services.AddSingleton(Configuration);
        }

        public static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance(Log.Logger).As<ILogger>();
            builder.AddMediatR(typeof(GridMeterCache).Assembly);
            builder.RegisterModule(new AutofacModule(Configuration));
            builder.RegisterInstance(BuildAppCache());
            builder.RegisterType<TimedHostedService>().AsSelf();
        }

        public static IAppCache BuildAppCache()
        {
            return new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }
    }
}
