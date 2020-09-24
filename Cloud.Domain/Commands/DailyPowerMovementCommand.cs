using Entities;
using MediatR;
using MediatR.Pipeline;
using ReadRepository.Cosmos;
using ReadRepository.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class DailyPowerMovementCommand : IRequest<PowerMovementDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyPowerMovementCommandHandler : IRequestHandler<DailyPowerMovementCommand, PowerMovementDaily>
    {
        private readonly IEnergySummaryDocumentReadRepository _energySummaryDocumentReadRepository;

        public DailyPowerMovementCommandHandler(
            IEnergySummaryDocumentReadRepository energySummaryDocumentReadRepository)
        {
            _energySummaryDocumentReadRepository = energySummaryDocumentReadRepository;
        }

        public async Task<PowerMovementDaily> Handle(DailyPowerMovementCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _energySummaryDocumentReadRepository.FetchPowerMovements(request.Date);

            var results = new PowerMovementDaily(
                date: request.Date,
                consumption: GetData(request.Date, dayData, a => a.ConsumptionWattHours),
                solarToHome: GetData(request.Date, dayData, a => a.SolarToHomeWattHours),
                solarToBattery: GetData(request.Date, dayData, a => a.SolarToBatteryWattHours),
                solarToGrid: GetData(request.Date, dayData, a => a.SolarToGridWattHours),
                gridToBattery: GetData(request.Date, dayData, a => a.GridToBatteryWattHours),
                gridToHome: GetData(request.Date, dayData, a => a.GridToHomeWattHours),
                batteryToHome: GetData(request.Date, dayData, a => a.BatteryToHomeWattHours),
                batteryToGrid: GetData(request.Date, dayData, a => a.BatteryToGridWattHours));

            return results;
        }

        private DayConsumptionSource GetData(DateTime date, IEnumerable<PowerMovementSummaryReadModel> dayData, Func<PowerMovementSummaryReadModel, decimal> p)
        {
            var times = Enumerable.Range(0, 24 * 12)
                .Select(i => TimeSpan.FromMinutes(5 * i)); // TODO this is for 5 minute intervals
            var phaseData = dayData
                .Where(a => a.IntervalStartIncluded.Date == date)
                .ToDictionary(a => a.IntervalStartIncluded.TimeOfDay, a => p(a));
            return new DayConsumptionSource(
                summary: times.Select(timeOfDay => (timeOfDay, GetTimeData(timeOfDay, phaseData))).ToList()
                );
        }

        private decimal? GetTimeData(TimeSpan timeOfDay, Dictionary<TimeSpan, decimal> phaseData)
        {
            if (phaseData.TryGetValue(timeOfDay, out decimal result))
                return result;
            return null;
        }
    }
}
