using Entities;
using LazyCache;
using LocalPublisher.Domain.Functions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SenecEntities;
using SenecEntitiesAdapter;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class SmartMeterEnergyCache : INotificationHandler<SmartMeterEnergy>
    {
        private readonly IWriteToCache _writeToCache;

        public SmartMeterEnergyCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(SmartMeterEnergy notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, "smartMeter");
            return Task.CompletedTask;
        }
    }
}
