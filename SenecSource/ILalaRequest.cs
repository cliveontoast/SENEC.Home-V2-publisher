using SenecEntities;
using System.Threading;
using System.Threading.Tasks;

namespace SenecSource
{
    public interface ILalaRequest : IEquipmentRequest
    {
    }
    public interface IEquipmentRequest
    {
        string? Content { get; set; }

        Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse;
    }
}