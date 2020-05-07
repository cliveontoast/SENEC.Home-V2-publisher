using Entities;
using MediatR;
using Repository;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageSummaryRepoStore : INotificationHandler<VoltageSummary>
    {
        private readonly ILogger _logger;
        private readonly IVoltageSummaryRepository _voltageSummaryRepository;

        public VoltageSummaryRepoStore(
            ILogger logger,
            IVoltageSummaryRepository voltageSummaryRepository)
        {
            _logger = logger;
            _voltageSummaryRepository = voltageSummaryRepository;
        }

        //public async Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        //{
        //    _logger.Information("Writing {Time}", notification.IntervalEndExcluded);
        //    _voltageSummaryRepository.AddAsync(notification);
        //    _logger.Information("Written {Time}", notification.IntervalEndExcluded);
        //    return Task.CompletedTask;
        //}

        // TODO - runs in MONO - revert to previous and test
        public Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            _logger.Information("Writing {Time}", notification.IntervalEndExcluded);
            try
            {
                _voltageSummaryRepository.Add(notification);
                _logger.Information("Written {Time}", notification.IntervalEndExcluded);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failure");
            }
            finally
            {
                _logger.Information("Finished handler");
            }
            return Task.CompletedTask;
        }
    }
}
