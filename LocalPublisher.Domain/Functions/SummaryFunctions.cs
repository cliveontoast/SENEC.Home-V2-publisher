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
            Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, IRepositoryEntity> buildSummary,
            string summaryType);
        DateTimeOffset GetLastTime(ConcurrentDictionary<long, string> collection);
        (DateTimeOffset Start, DateTimeOffset End) GetMinimumInterval(ConcurrentDictionary<long, string> collection);
        Task<Unit> Handle(CancellationToken cancellationToken);

        List<TSummaryEntity> FillSummary<TSummaryEntity, TSerialisedInput>(
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
        // TODO make a DI singleton
        private static object _lock = new object();

        private readonly IAppCache _cache;
        private readonly IMediator _mediator;
        private readonly IZoneProvider _zoneProvider;
        private ILogger _logger = null!;
        private IMinutesPerSummaryConfig _minutesPerSummaryConfig = null!;
        private Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, IRepositoryEntity> _buildSummary = null!;
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
            Func<ConcurrentDictionary<long, string>, DateTimeOffset, DateTimeOffset, List<string>, IRepositoryEntity> buildSummary,
            string summaryType)
        {
            _logger = logger;
            _minutesPerSummaryConfig = minutesPerSummaryConfig;
            _cache.GetOrAdd(cacheKey, () => new ConcurrentDictionary<long, string>());
            _buildSummary = buildSummary;
            _cacheKey = cacheKey;
            _summaryType = summaryType;
        }

        public async Task<Unit> Handle(CancellationToken cancellationToken)
        {
            try
            {
                List<IRepositoryEntity> tasks = GetSummaries();
                foreach (var summaries in tasks)
                {
                    _logger.Verbose("Publishing {StartTime}", summaries.IntervalStartIncluded);
                    await _mediator.Publish(summaries, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Logging here shouldn't be necessary, it should be caught elsewhere");
            }
            return Unit.Value;
        }

        private List<IRepositoryEntity> GetSummaries()
        {
            _logger.Verbose("Getting summaries");
            lock (_lock)
            {
                _logger.Verbose("Getting summaries - inside lock");
                var tasks = new List<IRepositoryEntity>();
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
                    var result = CreateSummary(collection, interval.Start, interval.End);
                    if (result == null) continue;
                    _logger.Verbose("Created {StartTime}", interval.Start);
                    tasks.Add(result);
                }
                _logger.Verbose("Getting summaries complete count {Count}", tasks.Count);
                return tasks;
            }
        }

        private IRepositoryEntity? CreateSummary(ConcurrentDictionary<long, string> collection, DateTimeOffset intervalStart, DateTimeOffset intervalEnd)
        {
            var removedTexts = new List<string>();
            try
            {
                return _buildSummary(collection, intervalStart, intervalEnd, removedTexts);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Create {SummaryType} summary error {@Values}", _summaryType, removedTexts);
                return null;
            }
        }

        public List<TSummaryEntity> FillSummary<TSummaryEntity, TSerialisedInput>(
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
                if (!collection.TryRemove(instant, out string textValue)) continue;
                removedTexts.Add(textValue);
                var original = JsonConvert.DeserializeObject<TSerialisedInput>(textValue);
                if (original == null) continue; // no way the programmer serialised nothing
                var moment = instant.ToEquipmentLocalTime(_zoneProvider);
                var entity = converter(moment, original);
                if (entity.IsValid)
                {
                    list.Add(entity);
                    postConvertAction?.Invoke(entity, instant);
                }
            }
            return list;
        }
    }
}
