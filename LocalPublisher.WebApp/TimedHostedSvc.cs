using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Collections.Generic;

namespace Domain
{
    public class TimedHostedService : TimedHostedSvc, IHostedService
    {
        public TimedHostedService(
            ILogger logger,
            IEnumerable<ITimedService> services,
            ILifetimeScope scope)
            : base(
                  logger,
                  services,
                  scope)
        {
        }
    }
}
