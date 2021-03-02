using System;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;

namespace TeslaPowerwallSource
{
    public interface IApiRequest
    {
        Task<MetersAggregates> GetMetersAggregatesAsync(CancellationToken token);
        Task<StateOfEnergy> GetStateOfEnergyAsync(CancellationToken token);
    }
}
