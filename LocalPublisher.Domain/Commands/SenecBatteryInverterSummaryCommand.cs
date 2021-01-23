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
    public class SenecBatteryInverterSummaryCommand : IRequest<Unit>
    {
    }

    public class SenecBatteryInverterSummaryCommandHandler : IRequestHandler<SenecBatteryInverterSummaryCommand, Unit>
    {
        private readonly ISummaryFunctions _summaryFunctions;
        private readonly IBatteryTemperatureAdapter _adapter;
        private readonly ISenecBatteryInverterTemperatureCompressConfig _config;
        private readonly ILogger _logger;

        public SenecBatteryInverterSummaryCommandHandler(
            ISummaryFunctions summaryFunctions,
            IBatteryTemperatureAdapter adapter,
            ISenecBatteryInverterTemperatureCompressConfig config,
            ILogger logger)
        {
            _summaryFunctions = summaryFunctions;
            _adapter = adapter;
            _config = config;
            _logger = logger;
            _summaryFunctions.Initialise(_logger, _config, BatteryInverterTemperatureCache.CacheKey, BuildTemperatureSummary, "batInvTemp");
        }

        public async Task<Unit> Handle(SenecBatteryInverterSummaryCommand request, CancellationToken cancellationToken)
        {
            return await _summaryFunctions.Handle(cancellationToken);
        }

        private InverterTemperatureSummary BuildTemperatureSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, List<string> removedTexts, CancellationToken cancellationToken)
        {
            var moments = _summaryFunctions.FillSummary<MomentBatteryInverterTemperatures, SenecEntities.BatteryObject1>(
                cancellationToken,
                collection,
                intervalStart,
                intervalEnd,
                (moment, meter) => _adapter.Convert(moment, meter).GetMoment(),
                removedTexts);

            var maximumValues = (int)(intervalEnd - intervalStart).TotalSeconds;

            var temperatureSensors = moments.Max(a => a.Temperatures.Count);
            Statistic[] statistics = Enumerable.Range(0, temperatureSensors)
                .Select(i => CreateStatistics(moments, (moment) => moment.Temperatures.Skip(i).First())).ToArray();

            var stats = new InverterTemperatureSummary(
                intervalStartIncluded: intervalStart,
                intervalEndExcluded: intervalEnd,
                secondsWithoutData: maximumValues - moments.Count,
                statistics
                );
            return stats;
        }

        private Statistic CreateStatistics(List<MomentBatteryInverterTemperatures> list, Func<MomentBatteryInverterTemperatures, decimal?> property)
        {
            var values = (
                from a in list
                let value = property(a)
                where value.HasValue
                orderby value
                select value.Value
                ).ToList();
            if (values.Count > 0)
            {
                var minimum = values.First();
                var maximum = values.Last();
                var midPoint = values.Count % 2 == 0
                    ? values.Count / 2 - 1
                    : values.Count / 2;
                var median = values[midPoint];
                return new Statistic(minimum, maximum, median, decimal.Round(values.Average(), 2));
            }
            return new Statistic(false);
        }
    }
}
