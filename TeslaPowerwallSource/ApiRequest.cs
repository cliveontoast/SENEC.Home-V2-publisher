using Newtonsoft.Json;
using Serilog;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;

namespace TeslaPowerwallSource
{
    public class ApiRequest : IApiRequest
    {
        private readonly ITimeProvider _time;
        private readonly ILogger _logger;
        private readonly ITeslaPowerwallSettings _settings;

        public ApiRequest(
            ITimeProvider time,
            ILogger logger,
            ITeslaPowerwallSettings settings)
        {
            _time = time;
            _logger = logger;
            _settings = settings;
        }

        public async Task<MetersAggregates> GetMetersAggregatesAsync(CancellationToken token)
        {
            return await Request<MetersAggregates>(token);
        }

        public async Task<StateOfEnergy> GetStateOfEnergyAsync(CancellationToken token)
        {
            return await Request<StateOfEnergy>(token);
        }

        public async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            var endpoint = GetEndPoint<TResponse>();
            return await RequestObject<TResponse>(endpoint, token);
        }

        private string GetEndPoint<TResponse>() where TResponse : WebResponse
        {
            var type = typeof(TResponse);
            if (type.Equals(typeof(MetersAggregates)))
                return "/api/meters/aggregates";
            else if (type.Equals(typeof(StateOfEnergy)))
                return "/api/system_status/soe";
            throw new NotImplementedException(type.FullName);
        }

        private async Task<(string? response, DateTimeOffset start, DateTimeOffset end)> Request(string endPoint, CancellationToken token)
        {
            using var client = new HttpClient();
            using var response = await GetResponse(client, endPoint, token);
                
            if (IsOk(response.response))
            {
                var result = await response.response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(result))
                    _logger.Error("Tesla powerwall returned success status code. Result was {IsNull} value of '{Result}'", result == null ? "null" : "not null", result);
                return (result, response.start, response.end);
            }
            return (null, response.start, response.end);
        }

        private async Task<TResponse> RequestObject<TResponse>(string endPoint, CancellationToken token) where TResponse : WebResponse
        {
            var response = await Request(endPoint, token);
            var result = Activator.CreateInstance<TResponse>();
            if (response.response == null || string.IsNullOrWhiteSpace(response.response))
            {
                _logger.Warning("tesla powerwall bad response {Response} {Start} {End}", response.response ?? "<nulL>", response.start, response.end);
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
            LocalStore(result);
            return result;
        }

        private void LocalStore<TResponse>(TResponse response) where TResponse : WebResponse
        {
            var text = JsonConvert.SerializeObject(response, Formatting.Indented);
            File.WriteAllText($@"C:\temp\localpublisher\teg\{response.ReceivedMilliseconds}.json", text);
        }

        private bool IsOk(HttpResponseMessage response)
        {
            if (response.Content != null && response.IsSuccessStatusCode)
                return true;

            _logger.Warning("tesla powerwall response was not ok {StatusCode} {@Response}", response.StatusCode, response);
            return false;
        }

        private async Task<ResponseTime> GetResponse(HttpClient client, string endPoint, CancellationToken token)
        {
            client.DefaultRequestHeaders.Clear();
            //client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            //client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,de;q=0.8");
            //client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            var start = _time.Now;
            var lalaResponse = await TryGet(client, endPoint, token);
            return (lalaResponse.value, start, lalaResponse.end);
        }

        private async Task<(HttpResponseMessage value, DateTimeOffset end)> TryGet(HttpClient client, string endPoint, CancellationToken token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.IP))
                {
                    var txt = $"Setting is unspecified '{_settings.IP}'";
                    _logger.Fatal(txt);
                    throw new Exception(txt);
                }
                return (await client.GetAsync($"https://{_settings.IP}{endPoint}", token), _time.Now);
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Couldn't fetch from Tesla powerwall");
                return (new HttpResponseMessage(), _time.Now);
            }
        }
    }

    public struct ResponseTime : IDisposable
    {
        public HttpResponseMessage response;
        public DateTimeOffset start;
        public DateTimeOffset end;

        public ResponseTime(HttpResponseMessage response, DateTimeOffset start, DateTimeOffset end)
        {
            this.response = response;
            this.start = start;
            this.end = end;
        }

        public override bool Equals(object obj)
        {
            return obj is ResponseTime other &&
                   EqualityComparer<HttpResponseMessage>.Default.Equals(response, other.response) &&
                   start.Equals(other.start) &&
                   end.Equals(other.end);
        }

        public override int GetHashCode()
        {
            int hashCode = 1909561119;
            hashCode = hashCode * -1521134295 + EqualityComparer<HttpResponseMessage>.Default.GetHashCode(response);
            hashCode = hashCode * -1521134295 + start.GetHashCode();
            hashCode = hashCode * -1521134295 + end.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out HttpResponseMessage response, out DateTimeOffset start, out DateTimeOffset end)
        {
            response = this.response;
            start = this.start;
            end = this.end;
        }

        public void Dispose()
        {
            response?.Dispose();
        }

        public static implicit operator (HttpResponseMessage response, DateTimeOffset start, DateTimeOffset end)(ResponseTime value)
        {
            return (value.response, value.start, value.end);
        }

        public static implicit operator ResponseTime((HttpResponseMessage response, DateTimeOffset start, DateTimeOffset end) value)
        {
            return new ResponseTime(value.response, value.start, value.end);
        }
    }
}
