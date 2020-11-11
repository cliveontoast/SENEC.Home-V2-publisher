using LocalPublisher.Domain.Functions;
using MediatR;
using SenecEntities;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class BatteryInverterTemperatureCache : INotificationHandler<BatteryInverterTemperature>
    {
        public const string CacheKey = "batteryInverterTemp";
        private readonly IWriteToCache _writeToCache;

        public BatteryInverterTemperatureCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(BatteryInverterTemperature notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, CacheKey);
            return Task.CompletedTask;
        }
    }
}
