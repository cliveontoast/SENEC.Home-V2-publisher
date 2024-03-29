﻿using Entities;
using LocalPublisher.Domain.Functions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageMomentsRepoStore :
        INotificationHandler<PersistenceInfo<IntervalOfMoments<MomentVoltage>>>, // TODO this should be a command, not publish
        INotificationHandler<IntervalOfMoments<MomentVoltage>>
    {
        //private readonly PersistToRepositoryFunctions<IntervalOfMoments<MomentVoltage>, VoltageMomentReadModel> _persistFunctions;

        public VoltageMomentsRepoStore(
            //ILogger logger,
            //IMediator mediator,
            //IAppCache cache,
            //IRepoConfig config,
            //IApplicationVersion versionConfig,
            //IVoltageMomentReadRepository voltageSummaryReadRepository,
            //IVoltageMomentRepository voltageSummaryRepository
            )
        {
            throw new Exception("Uses too much data");
            //_persistFunctions = new PersistToRepositoryFunctions<IntervalOfMoments<MomentVoltage>, VoltageMomentReadModel>(
            //    voltageSummaryReadRepository,
            //    voltageSummaryRepository,
            //    versionConfig,
            //    logger,
            //    config,
            //    cache,
            //    "persistVoltageMomentList",
            //    mediator,
            //    GetKeyExtensions.GetKey,
            //    GetKeyExtensions.GetKeyVersion2);
        }

        public async Task Handle(PersistenceInfo<IntervalOfMoments<MomentVoltage>> notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            //await _persistFunctions.Handle(notification, cancellationToken);
        }

        public async Task Handle(IntervalOfMoments<MomentVoltage> notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            //await _persistFunctions.Handle(notification, cancellationToken);
        }
    }
}
