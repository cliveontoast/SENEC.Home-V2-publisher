using Entities;
using MediatR;
using ReadRepository.ReadModel;

namespace LocalPublisher.Domain.Functions
{
    public class PersistenceInfo<TPersistedType> : INotification
        where TPersistedType : IIntervalEntity
    {
        private int _retryCount;
        public TPersistedType Summary { get; set; }

        public PersistenceInfo(TPersistedType summary)
        {
            Summary = summary;
        }

        public bool IsProcessing { get; set; }

        public int GetRetryCount()
        {
            return _retryCount;
        }

        public void Increment()
        {
            _retryCount++;
        }
    }
}
