using FroniusEntities;
using LazyCache;
using LocalPublisher.Domain.Functions;
using MediatR;
using SenecEntities;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class SolarPowerCache : INotificationHandler<SolarPower>
    {
        public const string CacheKey = "solarPower";
        private IWriteToCache _writeToCache;

        public SolarPowerCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(SolarPower notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, CacheKey);
            return Task.CompletedTask;
        }
    }
}
