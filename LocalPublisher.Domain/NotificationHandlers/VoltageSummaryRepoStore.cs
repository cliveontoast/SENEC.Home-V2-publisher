using Domain.NotificationHandlers;
using Entities;
using MediatR;
using ReadRepository.Repositories;
using Repository;
using Serilog;
using Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageSummaryRepoStore :
        INotificationHandler<VoltageSummary>,
        INotificationHandler<VoltageSummaryRetry>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IVoltageSummaryRepository _voltageSummaryRepository;
        private readonly ISenecCompressConfig _config;
        private readonly IApplicationVersion _versionConfig;
        private readonly IVoltageSummaryReadRepository _voltageSummaryReadRepository;

        public VoltageSummaryRepoStore(
            ILogger logger,
            IMediator mediator,
            ISenecCompressConfig config,
            IApplicationVersion versionConfig,
            IVoltageSummaryReadRepository voltageSummaryReadRepository,
            IVoltageSummaryRepository voltageSummaryRepository)
        {
            _logger = logger;
            _mediator = mediator;
            _config = config;
            _versionConfig = versionConfig;
            _voltageSummaryReadRepository = voltageSummaryReadRepository;
            _voltageSummaryRepository = voltageSummaryRepository;
        }

        public async Task Handle(VoltageSummaryRetry notification, CancellationToken cancellationToken)
        {
            await Handle(notification as VoltageSummary, cancellationToken);
        }

        public async Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            try
            {
                await FetchPersistedVersionAsync(notification);

                if (_versionConfig.PersistedNumber == _versionConfig.Number)
                    await WriteAsync(notification, cancellationToken);
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    var persistedRecord = _voltageSummaryReadRepository.Get(notification.GetKey());
                    if (persistedRecord == null)
                    {
                        await WriteAsync(notification, cancellationToken);
                        _versionConfig.PersistedNumber = _versionConfig.Number;
                    }
                }
            }
            catch (Microsoft.Azure.Cosmos.CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.Warning("Conflicted volt summary {Key}", notification.GetKey());
                return; 
            }
            catch (Exception e)
            {   
                _logger.Error(e, "Voltage summary persistence failure");
                var cosmosEx = e as Microsoft.Azure.Cosmos.CosmosException;
                await _mediator.Send(new PersistErrorCommand<VoltageSummary> 
                { 
                    Entity = notification, 
                    Response = cosmosEx?.StatusCode ?? System.Net.HttpStatusCode.Ambiguous,
                });
            }
        }

        private async Task WriteAsync(VoltageSummary notification, CancellationToken cancellationToken)
        {
            _logger.Information("Writing {Time}", notification.IntervalEndExcluded);
            await _voltageSummaryRepository.AddAsync(notification, cancellationToken);
            _logger.Information("Written {Time}", notification.IntervalEndExcluded);
        }

        private async Task FetchPersistedVersionAsync(VoltageSummary summary)
        {
            if (_versionConfig.PersistedNumber != null) return;

            var previousIntervalStart = summary.IntervalStartIncluded - (summary.IntervalEndExcluded - summary.IntervalStartIncluded);
            var persistedRecord =
                await _voltageSummaryReadRepository.Get(previousIntervalStart.GetIntervalKey())
                ?? await _voltageSummaryReadRepository.Get(summary.GetKey());
            if (persistedRecord == null)
            {
                _versionConfig.PersistedNumber = 0;
            }
            else
            {
                _versionConfig.PersistedNumber = persistedRecord.Version;
            }
        }
    }
}
