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
    public class VoltageSummaryRepoStore : INotificationHandler<VoltageSummary>
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

        // TODO - runs in MONO - revert to previous and test
        public Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            try
            {
                FetchPersistedVersion(notification);

                if (_versionConfig.PersistedNumber == _versionConfig.Number)
                    Write(notification);
                else
                {
                    WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle }, TimeSpan.FromSeconds(30));
                    var persistedRecord = _voltageSummaryReadRepository.Get(notification.GetKey());
                    if (persistedRecord == null)
                    {
                        Write(notification);
                        _versionConfig.PersistedNumber = _versionConfig.Number;
                    }
                }
            }
            catch (Microsoft.Azure.Cosmos.CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.Warning("Conflicted volt summary {Key}", notification.GetKey());
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                var cosmosEx = e as Microsoft.Azure.Cosmos.CosmosException;
                _mediator.Send(new PersistErrorCommand<VoltageSummary> 
                { 
                    Entity = notification, 
                    Response = cosmosEx?.StatusCode ?? System.Net.HttpStatusCode.Ambiguous,
                });
                _logger.Error(e, "Failure");
            }
            finally
            {
                _logger.Information("Finished handler");
            }

            return Task.CompletedTask;
        }

        private void Write(VoltageSummary notification)
        {
            _logger.Information("Writing {Time}", notification.IntervalEndExcluded);
            _voltageSummaryRepository.Add(notification);
            _logger.Information("Written {Time}", notification.IntervalEndExcluded);
        }

        private void FetchPersistedVersion(VoltageSummary summary)
        {
            if (_versionConfig.PersistedNumber != null) return;

            var previousIntervalStart = summary.IntervalStartIncluded - (summary.IntervalEndExcluded - summary.IntervalStartIncluded);
            var persistedRecord =
                _voltageSummaryReadRepository.Get(previousIntervalStart.GetIntervalKey())
                ?? _voltageSummaryReadRepository.Get(summary.GetKey());
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
