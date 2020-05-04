using SenecEntities;
using System.Threading;
using System.Threading.Tasks;

namespace SenecSource
{
    public interface ILalaRequest
    {
        string Content { get; set; }

        Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse;
    }
}