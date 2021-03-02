using LocalPublisher.Domain.Functions;
using MediatR;
using SenecEntities;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;

namespace Domain
{
    public class TeslaStateOfChargeCache : INotificationHandler<StateOfEnergyItem>
    {
        public const string CacheKey = "teslaStateOfCharge";
        private readonly IWriteToCache _writeToCache;

        public TeslaStateOfChargeCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(StateOfEnergyItem notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, CacheKey);
            return Task.CompletedTask;
        }
    }
}
