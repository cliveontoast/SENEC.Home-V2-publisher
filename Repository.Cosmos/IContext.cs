using Entities;
using Microsoft.Azure.Cosmos;
using Repository.Model;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Cosmos
{
    public interface IContext
    {
        Func<CancellationToken, Task<ItemResponse<EnergySummaryEntity>>> CreateItemAsync(EnergySummary energySummary);
        Func<CancellationToken, Task<ItemResponse<BatteryInverterTemperatureSummaryEntity>>> CreateItemAsync(InverterTemperatureSummary energySummary);
    }

    public class WriteContext : IContext
    {
        public enum TableDescriminators
        {
            ES, // energy summary
        }
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
    }
}

