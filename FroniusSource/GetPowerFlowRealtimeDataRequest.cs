using Newtonsoft.Json;
using SenecEntities;
using SenecSource;
using Serilog;
using Shared;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FroniusSource
{
    public interface IGetPowerFlowRealtimeDataRequest : IEquipmentRequest
    {
    }

    public class GetPowerFlowRealtimeDataRequest : IGetPowerFlowRealtimeDataRequest
    {
        private readonly ITimeProvider _time;
        private readonly ILogger _logger;
        private readonly IFroniusSettings _froniusSettings;

        public GetPowerFlowRealtimeDataRequest(
            ITimeProvider time,
            IFroniusSettings froniusSettings,
            ILogger logger)
        {
            _time = time;
            _logger = logger;
            _froniusSettings = froniusSettings;
        }

        public string? Content { get; set; }

        public async Task<(string? response, DateTimeOffset start, DateTimeOffset end)> Request(CancellationToken token)
        {
            using var client = new HttpClient();
            using var response = await GetResponse(client, token);

            if (IsOk(response.response))
            {
                var result = await response.response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(result))
                    _logger.Error("Fronius returned success status code. Result was {IsNull} value of '{Result}'", result == null ? "null" : "not null", result);
                return (result, response.start, response.end);
            }
            return (null, response.start, response.end);
        }

        public async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            var response = await Request(token);
            var result = Activator.CreateInstance<TResponse>();
            if (response.response == null || string.IsNullOrWhiteSpace(response.response))
            {
                _logger.Warning("Fronius bad response {Response} {Start} {End}", response.response ?? "<nulL>", response.start, response.end);
            }
            else
            {
                try
                {    
                    result = JsonConvert.DeserializeObject<TResponse>(response.response);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Cannot deserialise {Content} {Start} {End}", response.response, response.start, response.end);
                }
            }
            result.SentMilliseconds = response.start.ToUnixTimeMilliseconds();
            result.ReceivedMilliseconds = response.end.ToUnixTimeMilliseconds();
            return result;
        }

        private bool IsOk(HttpResponseMessage response)
        {
            if (response.Content != null && response.IsSuccessStatusCode)
                return true;

            _logger.Warning("Fronius response was not ok {StatusCode} {@Response}", response.StatusCode, response);
            return false;
        }

        private async Task<ResponseTime> GetResponse(HttpClient client, CancellationToken token)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            var start = _time.Now;
            var response = await TryGet(client, token);
            return (response.value, start, response.end);
        }

        private async Task<(HttpResponseMessage value, DateTimeOffset end)> TryGet(HttpClient client, CancellationToken token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_froniusSettings.IP))
                {
                    var txt = $"Fronius setting is unspecified '{_froniusSettings.IP}'";
                    _logger.Fatal(txt);
                    throw new Exception(txt);
                }
                client.Timeout = TimeSpan.FromMilliseconds(1);
                return (await client.GetAsync($"http://{_froniusSettings.IP}/solar_api/v1/GetPowerFlowRealtimeData.fcgi", token), _time.Now);
            }
            catch (TimeoutException e)
            {
                throw new BackOffPeriodException(e, TimeSpan.FromMinutes(1));
            }
            catch (HttpRequestException e) 
            when (e.InnerException is System.Net.WebException webex
                && webex.Status == System.Net.WebExceptionStatus.ConnectFailure
                && webex.InnerException is System.Net.Sockets.SocketException socex
                && (socex.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut
                    || socex.SocketErrorCode == System.Net.Sockets.SocketError.HostNotFound
                    || socex.SocketErrorCode == System.Net.Sockets.SocketError.HostUnreachable
                    ))
            {
                throw new BackOffPeriodException(e, TimeSpan.FromMinutes(1));
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Couldn't fetch from Fronius");
                return (new HttpResponseMessage(), _time.Now);
            }
        }
    }
}
