using Entities;
using MediatR;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

            var results = new VoltageSummaryDaily
            {
                Date = request.Date,
                L1 = GetPhase(request.Date, dayData, a => a.L1),
                L2 = GetPhase(request.Date, dayData, a => a.L2),
                L3 = GetPhase(request.Date, dayData, a => a.L3),
            };
            return results;
        }

        private DayVoltageSummary GetPhase(DateTime date, IEnumerable<VoltageSummaryReadModel> dayData, Func<VoltageSummaryReadModel, Statistic> p)
        {
            var times = Enumerable.Range(0, 24 * 12)
                .Select(i => TimeSpan.FromMinutes(5 * i)); // TODO this is for 5 minute intervals
            var phaseData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .ToDictionary(a => a.IntervalStartIncluded.TimeOfDay, a => p(a));
            return new DayVoltageSummary
            {
                PhaseSummary = times.Select(timeOfDay => (timeOfDay, GetTimeData(timeOfDay, phaseData))).ToList(),
            };
        }

        private Statistic? GetTimeData(TimeSpan timeOfDay, Dictionary<TimeSpan, Statistic> phaseData)
        {
            if (phaseData.TryGetValue(timeOfDay, out Statistic result))
                return result;
            return null;
        }
    }
}
