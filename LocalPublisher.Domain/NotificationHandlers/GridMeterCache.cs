using LocalPublisher.Domain.Functions;
using MediatR;
using SenecEntities;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class GridMeterCache : INotificationHandler<GridMeter>
    {
        public const string CacheKey = "gridmeter";
        private IWriteToCache _writeToCache;

        public GridMeterCache(
            IWriteToCache writeToCache)
        {
            _writeToCache = writeToCache;
        }

        public Task Handle(GridMeter notification, CancellationToken cancellationToken)
        {
            _writeToCache.Add(notification, CacheKey);
            return Task.CompletedTask;
        }
    }
}
