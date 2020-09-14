using Entities;
using Microsoft.Azure.Cosmos;
using Repository.Model;
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
    }

    public class WriteContext : IContext
    {
        public enum TableDescriminators
        {
            ES, // energy summary
        }
        private readonly Container _container;
        //private readonly IApplicationVersion _version;

        public WriteContext(
            //IApplicationVersion version,
            Container container)
        {
            _container = container;
            //_version = version;
        }

        public Func<CancellationToken, Task<ItemResponse<EnergySummaryEntity>>> CreateItemAsync(EnergySummary energySummary)
        {
            var persistedValue = new EnergySummaryEntity(energySummary, 5);// _version.Number);
            Func<CancellationToken, Task<ItemResponse<EnergySummaryEntity>>> obj = (CancellationToken c) => 
                _container.CreateItemAsync(persistedValue,
                    //new PartitionKey(persistedValue.Partition),
                    cancellationToken: c);
            return obj;
        }
    }
}

