using LazyCache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using SenecEntitesAdapter;
using Entities;
using MediatR;
using System.Threading.Tasks;
using Serilog;
using Shared;
using LocalPublisher.Domain.Functions;

namespace Domain
{
    public class SenecGridMeterSummaryCommand : IRequest<Unit>
    {
    }

    public class SenecGridMeterSummaryCommandHandler : IRequestHandler<SenecGridMeterSummaryCommand, Unit>
    {
        private readonly ISummaryFunctions _summaryFunctions;
        private readonly IGridMeterAdapter _gridMeterAdapter;
        private readonly ISenecVoltCompressConfig _config;
        private readonly ILogger _logger;

        public SenecGridMeterSummaryCommandHandler(
            ISummaryFunctions summaryFunctions,
            IGridMeterAdapter gridMeterAdapter,
            ISenecVoltCompressConfig config,
            ILogger logger)
        {
            _summaryFunctions = summaryFunctions;
            _gridMeterAdapter = gridMeterAdapter;
            _config = config;
            _logger = logger;
            _summaryFunctions.Initialise(_logger, _config, GridMeterCache.CacheKey, BuildVoltageSummary, "voltage");
        }

        public async Task<Unit> Handle(SenecGridMeterSummaryCommand request, CancellationToken cancellationToken)
        {
            return await _summaryFunctions.Handle(cancellationToken);
        }

        private VoltageSummary BuildVoltageSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, List<string> removedTexts, CancellationToken cancellationToken)
        {
            var list = _summaryFunctions.FillSummary<MomentVoltage, SenecEntities.Meter>(
                cancellationToken,
                collection,
                intervalStart,
                intervalEnd,
                (moment, meter) => _gridMeterAdapter.Convert(moment, meter).GetVoltageMoment(),
                removedTexts);

            var maximumValues = (int)(intervalEnd - intervalStart).TotalSeconds;
            var stats = new VoltageSummary(
                intervalStartIncluded: intervalStart,
                intervalEndExcluded: intervalEnd,
                l1: CreateStatistics(list, a => a.L1, maximumValues),
                l2: CreateStatistics(list, a => a.L2, maximumValues),
                l3: CreateStatistics(list, a => a.L3, maximumValues)
                );
            return stats;
        }

        private StatisticV1 CreateStatistics(List<MomentVoltage> list, Func<MomentVoltage, decimal> property, int maximumValues)
        {
            var values = (
                from a in list
                let value = property(a)
                orderby value
                select value
                ).ToList();
            var failures = maximumValues - values.Count;
            if (values.Count > 0)
            {
                var minimum = values.First();
                var maximum = values.Last();
                var midPoint = values.Count % 2 == 0
                    ? values.Count / 2 - 1
                    : values.Count / 2;
                var median = values[midPoint];
                return new StatisticV1(minimum, maximum, median, failures);
            }
            return new StatisticV1(failures);
        }
    }
}
