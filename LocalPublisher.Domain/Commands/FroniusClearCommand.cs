using FroniusEntities;
using FroniusSource;
using LazyCache;
using MediatR;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class FroniusClearCommand : IRequest<Unit>
    {
    }

    public class FroniusClearCommandHandler : IRequestHandler<FroniusClearCommand>
    {
        private readonly ILogger _logger;
        private IAppCache _cache;

        public FroniusClearCommandHandler(
            ILogger logger,
            IAppCache cache)
        {
            _logger = logger;
            _cache = cache;
            _cache.GetOrAdd(SolarPowerCache.CacheKey, () => new ConcurrentDictionary<long, string>());
        }

        public Task<Unit> Handle(FroniusClearCommand request, CancellationToken cancellationToken)
        {
            GetSolarValues();
            return Unit.Task;
        }

        private void GetSolarValues()
        {
            var cache = _cache.Get<ConcurrentDictionary<long, string>>(SolarPowerCache.CacheKey);
            
            _logger.Information("Fronius clearing {Count} {Average}", cache.Count, cache.Values.FirstOrDefault());
            cache.Clear();
        }
    }
}
