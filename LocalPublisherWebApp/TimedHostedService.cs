using Autofac;
using Domain;
using MediatR;
using Microsoft.Extensions.Hosting;
using Repository;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LocalPublisherWebApp
{
    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ILifetimeScope _scope;
        private readonly IEnumerable<ITimedService> _services;
        private List<Timer> _timers;

        public TimedHostedService(
            ILogger logger,
            IEnumerable<ITimedService> services,
            ILifetimeScope scope)
        {
            _logger = logger;
            _scope = scope;
            _services = services;
            _timers = new List<Timer>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Initialise();

            foreach (var item in _services)
            {
                _logger.Information("Timed Background Service {Name} is starting.", item.Command.Name);
                Type command = item.Command;
                _timers.Add(new Timer(async (state) =>
                {
                    using (var scope = _scope.BeginLifetimeScope())
                    {
                        var mediator = scope.Resolve<IMediator>();
                        var instance = scope.Resolve(command);
                        await mediator.Send(instance, cancellationToken);
                    }
                }, cancellationToken, TimeSpan.Zero, item.Period));
            }
        }

        private async Task Initialise()
        {
            using (var scope = _scope.BeginLifetimeScope())
            {
                var context = scope.Resolve<ILocalContext>();
                await context.SeedAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Timed Background Service is stopping.");

            foreach (var item in _timers)
            {
                item.Change(Timeout.Infinite, 0);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Task.WaitAll(_timers.Select(a => a.DisposeAsync().AsTask()).ToArray());
        }
    }
}
