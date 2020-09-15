using Entities;
using MediatR;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class DailyEnergySummaryCommand : IRequest<EnergySummaryDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyEnergySummaryCommandHandler : IRequestHandler<DailyEnergySummaryCommand, EnergySummaryDaily>
    {
        private readonly IEnergySummaryDocumentReadRepository _energySummaryDocumentReadRepository;

        public DailyEnergySummaryCommandHandler(
            IEnergySummaryDocumentReadRepository energySummaryDocumentReadRepository)
        {
            _energySummaryDocumentReadRepository = energySummaryDocumentReadRepository;
        }

        public async Task<EnergySummaryDaily> Handle(DailyEnergySummaryCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _energySummaryDocumentReadRepository.Fetch(request.Date);

            var results = new EnergySummaryDaily(
                date: request.Date,
                batteryCapacity: GetCapacity(request.Date, dayData)
                );
            return results;
        }

        private DayBatteryPercentage GetCapacity(DateTime date, IEnumerable<EnergySummaryReadModel> dayData)
        {
            var times = Enumerable.Range(0, 24 * 12)
                .Select(i => TimeSpan.FromMinutes(5 * i)); // TODO this is for 5 minute intervals
            var capacityData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .ToDictionary(a => a.IntervalStartIncluded.TimeOfDay, a => a.BatteryPercentageFull);
            return new DayBatteryPercentage(
                summary: times.Select(timeOfDay => (timeOfDay, GetTimeData(timeOfDay, capacityData))).ToList()
                );
        }

        private Statistic? GetTimeData(TimeSpan timeOfDay, Dictionary<TimeSpan, Statistic> data)
        {
            if (data.TryGetValue(timeOfDay, out Statistic result))
                return result;
            return null;
        }
    }
}
