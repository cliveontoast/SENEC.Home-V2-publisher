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
    public class DailyTemperatureSummaryCommand : IRequest<TemperatureInverterSummaryDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyTemperatureSummaryCommandHandler : IRequestHandler<DailyTemperatureSummaryCommand, TemperatureInverterSummaryDaily>
    {
        private readonly IInverterTemperatureSummaryDocumentReadRepository _inverterTemperatureSummaryDocumentReadRepository;

        public DailyTemperatureSummaryCommandHandler(
            IInverterTemperatureSummaryDocumentReadRepository inverterTemperatureSummaryDocumentReadRepository)
        {
            _inverterTemperatureSummaryDocumentReadRepository = inverterTemperatureSummaryDocumentReadRepository;
        }

        public async Task<TemperatureInverterSummaryDaily> Handle(DailyTemperatureSummaryCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _inverterTemperatureSummaryDocumentReadRepository.Fetch(request.Date);

            var results = new TemperatureInverterSummaryDaily(
                t1: GetTemperature(request.Date, dayData, a => a.MaximumTemperature.Skip(0).FirstOrDefault()),
                t2: GetTemperature(request.Date, dayData, a => a.MaximumTemperature.Skip(1).FirstOrDefault()),
                t3: GetTemperature(request.Date, dayData, a => a.MaximumTemperature.Skip(2).FirstOrDefault()),
                t4: GetTemperature(request.Date, dayData, a => a.MaximumTemperature.Skip(3).FirstOrDefault()),
                t5: GetTemperature(request.Date, dayData, a => a.MaximumTemperature.Skip(4).FirstOrDefault()),
                date: request.Date
                );
            return results;
        }

        private DayTemperatureSummary GetTemperature(DateTime date, IEnumerable<BatteryInverterTemperatureSummaryReadModel> dayData, Func<BatteryInverterTemperatureSummaryReadModel, decimal?> p)
        {
            var times = Enumerable.Range(0, 24 * 12)
                .Select(i => TimeSpan.FromMinutes(5 * i)); // TODO this is for 5 minute intervals
            var phaseData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .ToLookup(a => a.IntervalStartIncluded.TimeOfDay, a => p(a))
                .ToDictionary(a => a.Key, a => a.First());
            return new DayTemperatureSummary(
                phaseSummary: times.Select(timeOfDay => (timeOfDay, GetTimeData(timeOfDay, phaseData))).ToList()
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
