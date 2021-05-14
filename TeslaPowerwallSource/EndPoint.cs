using Serilog;
using System;
using System.Net.Http;
using TeslaEntities;

namespace TeslaPowerwallSource
{
    public class EndPoint : IEndPoint
    {
        private readonly ILogger _logger;

        public EndPoint(ILogger logger)
        {
            _logger = logger;
        }

        public string GetEndPoint<TResponse>() where TResponse : WebResponse
        {
            var type = typeof(TResponse);
            if (type.Equals(typeof(MetersAggregates)))
                return "/api/meters/aggregates";
            else if (type.Equals(typeof(StateOfEnergy)))
                return "/api/system_status/soe";
            else if (type.Equals(typeof(BasicAuthResponse)))
                return "/api/login/Basic";
            throw new NotImplementedException(type.FullName);
        }

        public bool IsOk(HttpResponseMessage response)
        {
            if (response.Content != null && response.IsSuccessStatusCode)
                return true;

            _logger.Warning("tesla powerwall response was not ok {StatusCode} {@Response}", response.StatusCode, response);
            return false;
        }
    }

    public interface IEndPoint
    {
        string GetEndPoint<TResponse>() where TResponse : WebResponse;
        bool IsOk(HttpResponseMessage response);
    }
}
