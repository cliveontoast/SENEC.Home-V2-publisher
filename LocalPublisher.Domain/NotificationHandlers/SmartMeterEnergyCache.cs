using LocalPublisher.Domain.Functions;
using MediatR;
using SenecEntities;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class SmartMeterEnergyCache : INotificationHandler<SmartMeterEnergy>
    {
        public const string CacheKey = "smartMeter";
        private readonly IWriteToCache _writeToCache;

        public SmartMeterEnergyCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(SmartMeterEnergy notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, CacheKey);
            return Task.CompletedTask;
        }
    }
}
