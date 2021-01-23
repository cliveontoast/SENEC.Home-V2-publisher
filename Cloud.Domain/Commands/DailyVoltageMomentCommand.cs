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
    public class DailyVoltageMomentCommand : IRequest<VoltageMomentDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyVoltageMomentCommandHandler : IRequestHandler<DailyVoltageMomentCommand, VoltageMomentDaily>
    {
        private readonly IVoltageMomentReadRepository _voltageMomentReadRepository;

        public DailyVoltageMomentCommandHandler(
            IVoltageMomentReadRepository voltageMomentReadRepository)
        {
            _voltageMomentReadRepository = voltageMomentReadRepository;
        }

        public async Task<VoltageMomentDaily> Handle(DailyVoltageMomentCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _voltageMomentReadRepository.Fetch(request.Date);

            var results = new VoltageMomentDaily(
                date: request.Date,
                l1: GetPhase(request.Date, dayData, a => a.IsValid ? a.L1 : (decimal?)null),
                l2: GetPhase(request.Date, dayData, a => a.IsValid ? a.L2 : (decimal?)null),
                l3: GetPhase(request.Date, dayData, a => a.IsValid ? a.L3 : (decimal?)null)
                );
            return results;
        }

        private DayVoltageMoments GetPhase(DateTime date, IEnumerable<VoltageMomentReadModel> dayData, Func<MomentVoltage, decimal?> p)
        {
            var times = Enumerable.Range(0, 24 * 60 * 60)
                .Select(i => TimeSpan.FromSeconds(i)); // TODO this is for 5 minute intervals
            var phaseData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .SelectMany(a => a.Moments.Where(a => a.IsValid))
                // problem on 27th sept 2020 in production data
                // while there could be two with a +00:00 and +08:00
                //.ToDictionary(a => a.IntervalStartIncluded.TimeOfDay, a => p(a));
                .ToLookup(a => a.Instant.TimeOfDay, a => p(a))
                .ToDictionary(a => a.Key, a => a.First());
            return new DayVoltageMoments(
                phaseMoments: times.Select(timeOfDay => (timeOfDay, GetTimeData(timeOfDay, phaseData))).ToList()
                );
        }

        private decimal? GetTimeData(TimeSpan timeOfDay, Dictionary<TimeSpan, decimal?> phaseData)
        {
            if (phaseData.TryGetValue(timeOfDay, out decimal? result))
                return result;
            return null;
        }
    }
}
