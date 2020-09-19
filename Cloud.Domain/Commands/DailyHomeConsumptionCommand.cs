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
    public class DailyHomeConsumptionCommand : IRequest<HomeConsumptionDaily>
    {
        public DateTime Date { get; set; }
    }

    public class DailyHomeConsumptionCommandHandler : IRequestHandler<DailyHomeConsumptionCommand, HomeConsumptionDaily>
    {
        private readonly IEnergySummaryDocumentReadRepository _energySummaryDocumentReadRepository;

        public DailyHomeConsumptionCommandHandler(
            IEnergySummaryDocumentReadRepository energySummaryDocumentReadRepository)
        {
            _energySummaryDocumentReadRepository = energySummaryDocumentReadRepository;
        }

        public async Task<HomeConsumptionDaily> Handle(DailyHomeConsumptionCommand request, CancellationToken cancellationToken)
        {
            var dayData = await _energySummaryDocumentReadRepository.Fetch(request.Date);

            var results = new HomeConsumptionDaily(
                date: request.Date,
                consumption: GetData(request.Date, dayData, a => a.ConsumptionWattHours),
                grid: GetData(request.Date, dayData, a => a.GridImportWattHours),
                solarConsumption: GetData(request.Date, dayData, a => Math.Min(a.ConsumptionWattHours - a.GridImportWattHours, a.SolarPowerGenerationWattHours)),
                batteryConsumption: GetData(request.Date, dayData, a => Math.Max(0,Math.Min(a.ConsumptionWattHours - a.GridImportWattHours - a.SolarPowerGenerationWattHours, a.BatteryDischargeWattHours))),
                batteryCharge: GetData(request.Date, dayData, a => a.BatteryChargeWattHours),

                solarExported: GetData(request.Date, dayData, a => Math.Max(0, a.SolarPowerGenerationWattHours 
                    - Math.Min(a.ConsumptionWattHours - a.GridImportWattHours, a.SolarPowerGenerationWattHours)
                    - a.BatteryChargeWattHours))
                );
            return results;
        }

        private DayConsumptionSource GetData(DateTime date, IEnumerable<EnergySummaryReadModel> dayData, Func<EnergySummaryReadModel, decimal> p)
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
