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
    public class DailyEquipmentStatesSummaryCommand : IRequest<EquipmentStatesSummaryDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyEquipmentStatesSummaryCommandHandler : IRequestHandler<DailyEquipmentStatesSummaryCommand, EquipmentStatesSummaryDaily>
    {
        private readonly IEquipmentStatesSummaryDocumentReadRepository _equipmentStatesSummaryDocumentReadRepository;

        public DailyEquipmentStatesSummaryCommandHandler(
            IEquipmentStatesSummaryDocumentReadRepository equipmentStatesSummaryDocumentReadRepository)
        {
            _equipmentStatesSummaryDocumentReadRepository = equipmentStatesSummaryDocumentReadRepository;
        }

        public async Task<EquipmentStatesSummaryDaily> Handle(DailyEquipmentStatesSummaryCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _equipmentStatesSummaryDocumentReadRepository.Fetch(request.Date);
            var stateList = dayData.SelectMany(a => a.States).Select(a => a.StateText).Distinct();
            var results = new EquipmentStatesSummaryDaily(
                summaries: GetStates(request.Date, dayData, stateList),
                date: request.Date
                );
            return results;
        }

        private IEnumerable<DayEquipmentStatesSummary> GetStates(DateTime date, IEnumerable<EquipmentStatesSummaryReadModel> dayData, IEnumerable<string> stateList)
        {
            var times = Enumerable.Range(0, 24 * 12)
                .Select(i => TimeSpan.FromMinutes(5 * i)); // TODO this is for 5 minute intervals
            var phaseData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .ToLookup(a => a.IntervalStartIncluded.TimeOfDay, a => a)
                .ToDictionary(a => a.Key, a => a.First());
            return stateList.Select(stateName => new DayEquipmentStatesSummary(
                stateName,
                times.Select(t => GetTimeData(t, phaseData, stateName))
                ));
        }

        private int? GetTimeData(TimeSpan timeOfDay, Dictionary<TimeSpan, EquipmentStatesSummaryReadModel> phaseData, string stateName)
        {
            if (!phaseData.TryGetValue(timeOfDay, out EquipmentStatesSummaryReadModel? states))
                return null;
            var result = states.States.SingleOrDefault(a => a.StateText == stateName).Count;
            return result == 0 ? (int?)null : result;
        }
    }
}
