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
    public class DailyVoltageSummaryCommand : IRequest<VoltageSummaryDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyVoltageSummaryCommandHandler : IRequestHandler<DailyVoltageSummaryCommand, VoltageSummaryDaily>
    {
        private readonly IVoltageSummaryDocumentReadRepository _voltageSummaryDocumentReadRepository;

        public DailyVoltageSummaryCommandHandler(
            IVoltageSummaryDocumentReadRepository voltageSummaryDocumentReadRepository)
        {
            _voltageSummaryDocumentReadRepository = voltageSummaryDocumentReadRepository;
        }

        public async Task<VoltageSummaryDaily> Handle(DailyVoltageSummaryCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _voltageSummaryDocumentReadRepository.Fetch(request.Date);

            var results = new VoltageSummaryDaily(
                date: request.Date,
                l1: GetPhase(request.Date, dayData, a => a.L1),
                l2: GetPhase(request.Date, dayData, a => a.L2),
                l3: GetPhase(request.Date, dayData, a => a.L3)
                );
            return results;
        }

        private DayVoltageSummary GetPhase(DateTime date, IEnumerable<VoltageSummaryReadModel> dayData, Func<VoltageSummaryReadModel, StatisticV1> p)
        {
            var times = Enumerable.Range(0, 24 * 12)
                .Select(i => TimeSpan.FromMinutes(5 * i)); // TODO this is for 5 minute intervals
            var phaseData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .ToDictionary(a => a.IntervalStartIncluded.TimeOfDay, a => p(a));
            return new DayVoltageSummary(
                phaseSummary: times.Select(timeOfDay => (timeOfDay, GetTimeData(timeOfDay, phaseData))).ToList()
                );
        }

        private StatisticV1? GetTimeData(TimeSpan timeOfDay, Dictionary<TimeSpan, StatisticV1> phaseData)
        {
            if (phaseData.TryGetValue(timeOfDay, out StatisticV1 result))
                return result;
            return null;
        }
    }
}
