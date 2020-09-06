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

#nullable enable
namespace LocalPublisherMono
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting up. This logged line only appears on the console.");

                IServiceCollection services = new ServiceCollection();
                var cb = BuildContainer(services);
                var startup = new Startup();
                startup.ConfigureServices(services);
                startup.ConfigureContainer(cb);

                Go(cb.Build());
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

        private static void Go(IContainer container)
        {
            using var scope = container.BeginLifetimeScope();
            using var tokenSource = new CancellationTokenSource();

            var service = scope.Resolve<TimedHostedService>();
            service.StartAsync(tokenSource.Token).Wait();
            var cancelTask = Task.Factory.StartNew(() =>
            {
                Log.Information("Press ENTER to quit");
                Console.ReadLine();
                tokenSource.Cancel();
            });
            Task.WaitAll(cancelTask);
            Log.Information("Exiting");
        }

        public static ContainerBuilder BuildContainer(IServiceCollection services)
        {
            var factory = new AutofacServiceProviderFactory();
            var cb = factory.CreateBuilder(services);
            return cb;
        }
    }
}
#nullable disable