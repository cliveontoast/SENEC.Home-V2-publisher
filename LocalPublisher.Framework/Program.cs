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
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationCallback;
                Go(cb.Build());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.Information("Toilets flush when they are finished");
                Log.CloseAndFlush();
            }
        }

        private static bool CheckValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Log.Debug("yep.");
            return true;
        }

        private static void Go(IContainer container)
        {
            using var scope = container.BeginLifetimeScope();
            using var tokenSource = new CancellationTokenSource();

            var service = scope.Resolve<TimedHostedSvc>();
            service.StartAsync(tokenSource.Token).Wait();
            var cancelTask = Task.Factory.StartNew(() =>
            {
                Log.Information("Press ENTER to quit");
                Console.ReadLine();
                service.StopAsync(tokenSource.Token);
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