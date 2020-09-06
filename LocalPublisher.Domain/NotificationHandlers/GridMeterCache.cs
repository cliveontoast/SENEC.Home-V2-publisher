﻿using Entities;
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
    public class GridMeterCache : INotificationHandler<GridMeter>
    {
        private IWriteToCache _writeToCache;

        public GridMeterCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(GridMeter notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, "gridmeter");
            return Task.CompletedTask;
        }
    }
}
