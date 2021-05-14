using Domain;
using Entities;
using LazyCache;
using MediatR;
using Newtonsoft.Json;
using Serilog;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalPublisher.Domain.Functions
{

    public interface ISummaryFunctions
    {
        void Initialise(ILogger _logger, IMinutesPerSummaryConfig minutesPerSummaryConfig, string cacheKey,
            Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IIntervalEntity> buildSummary,
            string summaryType);
        void Initialise(ILogger _logger, IMinutesPerSummaryConfig minutesPerSummaryConfig, string cacheKey,
            Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IEnumerable<IIntervalEntity>> buildSummary,
            string summaryType);
        DateTimeOffset GetLastTime(ConcurrentDictionary<long, string> collection);
        (DateTimeOffset Start, DateTimeOffset End) GetMinimumInterval(ConcurrentDictionary<long, string> collection);
        Task<Unit> Handle(CancellationToken cancellationToken);

        List<TSummaryEntity> FillSummary<TSummaryEntity, TSerialisedInput>(
            CancellationToken cancellationToken,
            ConcurrentDictionary<long, string> collection,
            DateTimeOffset intervalStart,
            DateTimeOffset intervalEnd,
            Func<DateTimeOffset, TSerialisedInput, TSummaryEntity> converter,
            List<string> removedTexts,
            Action<long, long>? lowerUpperBoundExtras = null,
            Action<TSummaryEntity, long>? postConvertAction = null
            ) where TSummaryEntity : IIsValid;
    }

    public class SummaryFunctions : ISummaryFunctions
    {
        public const string summaryIssue = "Summary:: ";
        // TODO make a DI singleton
        private static object _lock = new object();

        private readonly IAppCache _cache;
        private readonly IMediator _mediator;
        private readonly IZoneProvider _zoneProvider;
        private ILogger _logger = null!;
        private IMinutesPerSummaryConfig _minutesPerSummaryConfig = null!;
        private Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IEnumerable<IIntervalEntity>> _buildSummary = null!;
        private string _cacheKey = null!;
        private string _summaryType = null!;

        public SummaryFunctions(
            IAppCache cache,
            IMediator mediator,
            IZoneProvider zoneProvider)
        {
            _cache = cache;
            _mediator = mediator;
            _zoneProvider = zoneProvider;
        }

        public (DateTimeOffset Start, DateTimeOffset End) GetMinimumInterval(
            ConcurrentDictionary<long, string> collection)
        {
            var firstItemUnixTime = collection.Keys.Min();
            DateTimeOffset firstTime = firstItemUnixTime.ToEquipmentLocalTime(_zoneProvider);
            _logger.Verbose("Minimum in collection {Minimum} {Time}", firstItemUnixTime, firstTime);
            var frequency = TimeSpan.FromMinutes(_minutesPerSummaryConfig.MinutesPerSummary);
            var minIntoInterval = firstTime.TimeOfDay.Ticks % frequency.Ticks;
            var intervalStart = firstTime.AddTicks(-minIntoInterval).ToEquipmentLocalTime(_zoneProvider);
            var intervalEnd = (intervalStart + frequency).ToEquipmentLocalTime(_zoneProvider);
            return (intervalStart, intervalEnd);
        }

        public DateTimeOffset GetLastTime(ConcurrentDictionary<long, string> collection)
        {
            var lastItem = collection.Keys.Max();
            return lastItem.ToEquipmentLocalTime(_zoneProvider);
        }

        public void Initialise(ILogger logger, IMinutesPerSummaryConfig minutesPerSummaryConfig, string cacheKey,
            Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IEnumerable<IIntervalEntity>> buildSummaries,
            string summaryType)
        {
            _logger = logger;
            _minutesPerSummaryConfig = minutesPerSummaryConfig;
            _cacheKey = cacheKey;
            _cache.GetOrAdd(_cacheKey, () => new ConcurrentDictionary<long, string>(), DateTimeOffset.MaxValue);
            _buildSummary = buildSummaries;
            _summaryType = summaryType;
        }

        public void Initialise(ILogger logger, IMinutesPerSummaryConfig minutesPerSummaryConfig, string cacheKey,
            Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IIntervalEntity> buildSummary,
            string summaryType)
        {
            Initialise(logger, minutesPerSummaryConfig, cacheKey, ToSummariesList(buildSummary), summaryType);
        }

        private Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IEnumerable<IIntervalEntity>>
            ToSummariesList(Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IIntervalEntity> buildSummary)
        {
            return new Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, CancellationToken, IEnumerable<IIntervalEntity>>((a, b, c, d, e) 
                => new IIntervalEntity[1] { buildSummary(a, b, c, d, e) });
        }

        public async Task<Unit> Handle(CancellationToken cancellationToken)
        {
            try
            {
                List<IIntervalEntity> tasks = GetSummaries(cancellationToken);
                if (tasks.Any())
                {
                    await PersistPublisherStatus(tasks.First().IntervalEndExcluded);
                }
                foreach (var summaries in tasks)
                {
                    _logger.Verbose("Publishing {Type} {StartTime}", summaries.GetType(), summaries.IntervalStartIncluded);
                    await _mediator.Publish(summaries, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
            }
            return Unit.Value;
        }

        private async Task PersistPublisherStatus(DateTimeOffset intervalEndExcluded)
        {
            await _mediator.Publish(new Publisher(Environment.MachineName, intervalEndExcluded.ToEquipmentLocalTime(_zoneProvider)));
        }

        private List<IIntervalEntity> GetSummaries(CancellationToken cancellationToken)
        {
            _logger.Verbose("Getting summaries");
            lock (_lock)
            {
                _logger.Verbose("Getting summaries - inside lock");
                var tasks = new List<IIntervalEntity>();
                var collection = _cache.Get<ConcurrentDictionary<long, string>>(_cacheKey);
                while (collection != null && collection.Count > 0)
                {
                    _logger.Verbose("grid meter loop count {Count}", collection.Count);
                    var interval = GetMinimumInterval(collection);
                    var intervalPlusBuffer = interval.End.AddSeconds(10);
                    var internalLastAdded = GetLastTime(collection);
                    var isBufferBeyondLastItem = intervalPlusBuffer >= internalLastAdded;
                    _logger.Verbose("{intervalPlusBuffer} >= {internalLastAdded} is {Truthiness}", intervalPlusBuffer, internalLastAdded, isBufferBeyondLastItem);
                    if (isBufferBeyondLastItem) break;

                    _logger.Verbose("Creating {StartTime}", interval.Start);
                    var result = CreateSummary(collection, interval.Start, interval.End, cancellationToken);
                    _logger.Verbose("Created {StartTime}", interval.Start);
                    foreach (var item in result)
                    {
                        if (item == null) continue;
                        tasks.Add(item);
                    }
                }
                _logger.Verbose("Getting summaries complete count {Count}", tasks.Count);
                return tasks;
            }
        }

        private IEnumerable<IIntervalEntity?> CreateSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd, CancellationToken cancellationToken)
        {
            var removedTexts = new List<string>();
            try
            {
                return _buildSummary(collection, intervalStart, intervalEnd, removedTexts, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Create {SummaryType} summary error {@Values}", _summaryType, removedTexts);
                return Enumerable.Empty<IIntervalEntity?>();
            }
        }

        public List<TSummaryEntity> FillSummary<TSummaryEntity, TSerialisedInput>(
            CancellationToken cancellationToken,
            ConcurrentDictionary<long, string> collection,
            DateTimeOffset intervalStart,
            DateTimeOffset intervalEnd,
            Func<DateTimeOffset, TSerialisedInput, TSummaryEntity> converter,
            List<string> removedTexts,
            Action<long,long>? lowerUpperBoundExtras = null,
            Action<TSummaryEntity,long>? postConvertAction = null
            ) where TSummaryEntity : IIsValid
        {
            var list = new List<TSummaryEntity>();
            var lowerBound = intervalStart.ToUnixTimeSeconds();
            var upperBound = intervalEnd.ToUnixTimeSeconds();
            lowerUpperBoundExtras?.Invoke(lowerBound, upperBound);
            for (long instant = lowerBound; instant < upperBound; instant++)
            {
                var asDateTime = DateTimeOffset.FromUnixTimeSeconds(instant).LocalDateTime;
                if (!collection.TryRemove(instant, out string textValue))
                {
                    _logger.Verbose(summaryIssue + $"Missing {asDateTime}");
                    continue;
                }
                removedTexts.Add(textValue);
                var original = JsonConvert.DeserializeObject<TSerialisedInput>(textValue);
                if (original == null)
                {
                    _logger.Verbose(summaryIssue + $"Deserialisation failure {asDateTime}");
                    continue; // no way the programmer serialised nothing
                }
                var moment = instant.ToEquipmentLocalTime(_zoneProvider);
                var entity = converter(moment, original);
                if (entity.IsValid)
                {
                    list.Add(entity);
                    postConvertAction?.Invoke(entity, instant);
                }
                else
                {
                    _logger.Verbose(summaryIssue + $"Invalid {asDateTime}");
                }
            }
            _logger.Verbose(summaryIssue + $"Publishing {nameof(TSummaryEntity)} count {list.Count}");
            _mediator.Publish(new IntervalOfMoments<TSummaryEntity>(intervalStart, intervalEnd, list), cancellationToken);
            return list;
        }
    }
}
