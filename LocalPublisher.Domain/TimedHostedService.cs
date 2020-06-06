using Autofac;
using MediatR;
using Microsoft.Extensions.Hosting;
using Repository;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class TimedHostedService : IHostedService, IDisposable
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
            await Initialise(cancellationToken);

            foreach (var item in _services)
            {
                _logger.Information("Timed Background Service {Name} is starting.", item.Command.Name);
                Type command = item.Command;
                _timers.Add(new Timer(async (state) =>
                {
                    try
                    {
                        using (var scope = _scope.BeginLifetimeScope())
                        {
                            var mediator = scope.Resolve<IMediator>();
                            var instance = scope.Resolve(command);
                            await mediator.Send(instance, cancellationToken);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Timer {Name} Failed", command.Name);
                    }
                }, cancellationToken, TimeSpan.Zero, item.Period));
            }
        }

        private async Task Initialise(CancellationToken cancellationToken)
        {
            using (var scope = _scope.BeginLifetimeScope())
            {
                var context = scope.Resolve<ILocalContext>();
                await context.SeedAsync(cancellationToken);
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

        public virtual void Dispose()
        {
            foreach (var timer in _timers)
            {
                timer.Dispose();
            }
        }
    }
}
