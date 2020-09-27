using Autofac;
using MediatR;
using Microsoft.Extensions.Hosting;
using Repository;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ILifetimeScope _scope;
        private readonly IEnumerable<ITimedService> _services;
        private Dictionary<string, Timer> _timers;

        private class TimerCallbackState
        {
            public Type command;
            public CancellationToken cancellationToken;
            public TimeSpan period;
            public int pauseTimes;
            internal bool isLastCallbackBackoff;

            public TimerCallbackState(Type command, CancellationToken cancellationToken, TimeSpan period, int pauseTimes)
            {
                this.command = command;
                this.cancellationToken = cancellationToken;
                this.period = period;
                this.pauseTimes = pauseTimes;
            }
        }

        public TimedHostedService(
            ILogger logger,
            IEnumerable<ITimedService> services,
            ILifetimeScope scope)
        {
            _logger = logger;
            _scope = scope;
            _services = services;
            _timers = new Dictionary<string, Timer>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Init timed host service");
            await Initialise(cancellationToken);

            foreach (var item in _services)
            {
                _logger.Information("Timed Background Service {Name} is starting.", item.Command.Name);
                Type command = item.Command;
                var callbackState = new TimerCallbackState(command: command, cancellationToken: cancellationToken, item.Period, 0);
                _timers.Add(command.Name, new Timer(async (state) => await timerCallback(state), callbackState, TimeSpan.FromSeconds(1), item.Period));
            }
        }

        private async Task timerCallback(object state)
        {
            var stateObject = (TimerCallbackState)state;
            if (stateObject.cancellationToken.IsCancellationRequested) return;
            try
            {
                using (var scope = _scope.BeginLifetimeScope())
                {
                    var mediator = scope.Resolve<IMediator>();
                    var instance = scope.Resolve(stateObject.command);
                    if (instance is IRequest<TimeSpan?> backoffInstance)
                    {
                        var result = await mediator.Send(backoffInstance, stateObject.cancellationToken);
                        if (result != null)
                        {
                            _logger.Information("Timer backing off");
                            _timers[stateObject.command.Name].Change(result.Value, result.Value);
                            stateObject.isLastCallbackBackoff = true;
                        }
                        else if (stateObject.isLastCallbackBackoff)
                        {
                            _logger.Information("Timer coming back on");
                            stateObject.isLastCallbackBackoff = false;
                            _timers[stateObject.command.Name].Change(TimeSpan.FromSeconds(1), _services.First(a => a.Command == stateObject.command).Period);
                        }
                    }
                    else
                        await mediator.Send(instance, stateObject.cancellationToken);
                    
                }
            }
            catch (Exception e)
            {
                if (!stateObject.cancellationToken.IsCancellationRequested)
                    _logger.Error(e, "Timer {Name} Failed", stateObject.command.Name);
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
            _logger.Error("Not an error... timed Background Service is stopping.");
            foreach (var item in _timers.Values)
            {
                item.Change(Timeout.Infinite, 0);
            }
            _logger.Error("Stop completed");
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
        }
    }
}
