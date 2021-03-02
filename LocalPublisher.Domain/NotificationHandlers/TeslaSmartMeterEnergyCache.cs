using LocalPublisher.Domain.Functions;
using MediatR;
using SenecEntities;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;

namespace Domain
{
    public class TeslaSmartMeterEnergyCache : INotificationHandler<MetersAggregatesItem>
    {
        public const string CacheKey = "teslaSmartMeter";
        private readonly IWriteToCache _writeToCache;

        public TeslaSmartMeterEnergyCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(MetersAggregatesItem notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, CacheKey);
            return Task.CompletedTask;
        }
    }
}
