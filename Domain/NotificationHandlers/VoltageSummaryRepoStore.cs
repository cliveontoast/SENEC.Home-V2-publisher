using Entities;
using MediatR;
using Repository;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageSummaryRepoStore : INotificationHandler<VoltageSummary>
    {
        private readonly IVoltageSummaryRepository _voltageSummaryRepository;

        public VoltageSummaryRepoStore(
            IVoltageSummaryRepository voltageSummaryRepository)
        {
            _voltageSummaryRepository = voltageSummaryRepository;
        }

        public async Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            await _voltageSummaryRepository.AddAsync(notification);
        }
    }
}
