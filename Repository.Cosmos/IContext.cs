using Entities;
using Microsoft.Azure.Cosmos;
using Repository.Model;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Cosmos
{
    public interface IContext
    {
        Func<CancellationToken, Task<ItemResponse<EnergySummaryEntity>>> CreateItemAsync(EnergySummary energySummary);
        Func<CancellationToken, Task<ItemResponse<BatteryInverterTemperatureSummaryEntity>>> CreateItemAsync(InverterTemperatureSummary energySummary);
        Func<CancellationToken, Task<ItemResponse<EquipmentStatesSummaryEntity>>> CreateItemAsync(EquipmentStatesSummary energySummary);
        Func<CancellationToken, Task<ItemResponse<PublisherEntity>>> CreateItemAsync(Publisher energySummary);
        Func<CancellationToken, Task<ItemResponse<PublisherEntity>>> ReplaceItemAsync(Publisher publisher);
        Func<CancellationToken, Task<ItemResponse<IntervalOfMomentsEntity<VoltageMomentEntity>>>> CreateItemAsync(IntervalOfMoments<MomentVoltage> energySummary);
        Func<CancellationToken, Task<ItemResponse<IntervalOfMomentsEntity<VoltageMomentEntity>>>> ReplaceItemAsync(IntervalOfMoments<MomentVoltage> energySummary);
    }

    public class WriteContext : IContext
    {
        private readonly Container _container;
        private readonly IApplicationVersion _version;

        public WriteContext(
            IApplicationVersion version,
            Container container)
        {
            _container = container;
            _version = version;
        }

        public Func<CancellationToken, Task<ItemResponse<EnergySummaryEntity>>> CreateItemAsync(EnergySummary energySummary)
        {
            var persistedValue = new EnergySummaryEntity(energySummary, _version.Number);
            Func<CancellationToken, Task<ItemResponse<EnergySummaryEntity>>> obj = (CancellationToken c) => 
                _container.CreateItemAsync(persistedValue,
                    cancellationToken: c);
            return obj;
        }

        public Func<CancellationToken, Task<ItemResponse<BatteryInverterTemperatureSummaryEntity>>> CreateItemAsync(InverterTemperatureSummary energySummary)
        {
            var persistedValue = new BatteryInverterTemperatureSummaryEntity(energySummary, _version.Number);
            Func<CancellationToken, Task<ItemResponse<BatteryInverterTemperatureSummaryEntity>>> obj = (CancellationToken c) => 
                _container.CreateItemAsync(persistedValue,
                    cancellationToken: c);
            return obj;
        }

        public Func<CancellationToken, Task<ItemResponse<EquipmentStatesSummaryEntity>>> CreateItemAsync(EquipmentStatesSummary energySummary)
        {
            var persistedValue = new EquipmentStatesSummaryEntity(energySummary, _version.Number);
            Func<CancellationToken, Task<ItemResponse<EquipmentStatesSummaryEntity>>> obj = (CancellationToken c) => 
                _container.CreateItemAsync(persistedValue,
                    cancellationToken: c);
            return obj;
        }

        public Func<CancellationToken, Task<ItemResponse<PublisherEntity>>> CreateItemAsync(Publisher publisher)
        {
            var persistedValue = new PublisherEntity(publisher, _version.Number);
            Func<CancellationToken, Task<ItemResponse<PublisherEntity>>> obj = (CancellationToken c) =>
                _container.CreateItemAsync(persistedValue,
                    cancellationToken: c);
            return obj;
        }

        public Func<CancellationToken, Task<ItemResponse<PublisherEntity>>> ReplaceItemAsync(Publisher publisher)
        {
            var persistedValue = new PublisherEntity(publisher, _version.Number);
            Func<CancellationToken, Task<ItemResponse<PublisherEntity>>> obj = (CancellationToken c) =>
                _container.ReplaceItemAsync(persistedValue, persistedValue.Id,
                    cancellationToken: c);
            return obj;
        }

        public Func<CancellationToken, Task<ItemResponse<IntervalOfMomentsEntity<VoltageMomentEntity>>>> CreateItemAsync(IntervalOfMoments<MomentVoltage> energySummary)
        {
            var persistedValue = new IntervalOfMomentsEntity<VoltageMomentEntity>(energySummary, _version.Number, energySummary.Moments, (a) => new VoltageMomentEntity((MomentVoltage)a));
            Func<CancellationToken, Task<ItemResponse<IntervalOfMomentsEntity<VoltageMomentEntity>>>> obj = (CancellationToken c) =>
                _container.CreateItemAsync(persistedValue,
                    cancellationToken: c);
            return obj;
        }

        public Func<CancellationToken, Task<ItemResponse<IntervalOfMomentsEntity<VoltageMomentEntity>>>> ReplaceItemAsync(IntervalOfMoments<MomentVoltage> energySummary)
        {
            var persistedValue = new IntervalOfMomentsEntity<VoltageMomentEntity>(energySummary, _version.Number, energySummary.Moments, (a) => new VoltageMomentEntity((MomentVoltage)a));
            Func<CancellationToken, Task<ItemResponse<IntervalOfMomentsEntity<VoltageMomentEntity>>>> obj = (CancellationToken c) =>
                _container.ReplaceItemAsync(persistedValue, persistedValue.Id,
                    cancellationToken: c);
            return obj;
        }
    }
}

