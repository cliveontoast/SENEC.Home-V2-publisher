using LazyCache;
using Newtonsoft.Json;
using Serilog;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IClient _client;
        private readonly IEndPoint _endPoint;

        public ApiRequest(
            ITimeProvider time,
            ILogger logger,
            IClient client,
            IEndPoint endPoint,
            ITeslaPowerwallSettings settings)
        {
            _time = time;
            _logger = logger;
            _settings = settings;
            _client = client;
            _endPoint = endPoint;
        }

        public async Task<MetersAggregates> GetMetersAggregatesAsync(CancellationToken token)
        {
            return await Request<MetersAggregates>(token);
        }

        public async Task<StateOfEnergy> GetStateOfEnergyAsync(CancellationToken token)
        {
            return await Request<StateOfEnergy>(token);
        }

        private async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            var endpoint = _endPoint.GetEndPoint<TResponse>();
            return await RequestObject<TResponse>(endpoint, token);
        }

        private async Task<TResponse> RequestObject<TResponse>(string endPoint, CancellationToken token) where TResponse : WebResponse
        {
            var response = await Request(endPoint, token);
            var result = Activator.CreateInstance<TResponse>();
            if (response.response == null || string.IsNullOrWhiteSpace(response.response))
            {
                _logger.Warning(Client.LOGGING + "tesla powerwall bad response {Response} {Start} {End}", response.response ?? "<nulL>", response.start, response.end);
            }
            else
            {
                try
                {
                    result = JsonConvert.DeserializeObject<TResponse>(response.response);
                }
                catch (Exception e)
                {
                    _logger.Error(e, Client.LOGGING + "Cannot deserialise {Content} {Start} {End}", response.response, response.start, response.end);
                }
            }
            result.SentMilliseconds = response.start.ToUnixTimeMilliseconds();
            result.ReceivedMilliseconds = response.end.ToUnixTimeMilliseconds();
            LocalStore(result);
            return result;
        }

        private async Task<(string? response, DateTimeOffset start, DateTimeOffset end)> Request(string endPoint, CancellationToken token)
        {
            var client = _client.Build(token);
            using var response = await GetResponse(client, endPoint, token);
            var isOk = _endPoint.IsOk(response.response);
            _logger.Information(Client.LOGGING + $"Client {client.GetHashCode()} response is {response.response.StatusCode}");
            if (!isOk)
                return (null, response.start, response.end);

            var result = await response.response.Content.ReadAsStringAsync();
            _logger.Debug(result);
            if (string.IsNullOrWhiteSpace(result))
                _logger.Error(Client.LOGGING + "Tesla powerwall returned success status code. Result was {IsNull} value of '{Result}'", result == null ? "null" : "not null", result);
            return (result, response.start, response.end);
        }

        private void LocalStore<TResponse>(TResponse response) where TResponse : WebResponse
        {
            if (_settings.WriteFolder.Length == 0) return;

            var text = JsonConvert.SerializeObject(response, Formatting.Indented);
            File.WriteAllText(Path.Combine(".", _settings.WriteFolder, $"{response.ReceivedMilliseconds}.json"), text);
        }

        private async Task<ResponseTime> GetResponse(HttpClient client, string endPoint, CancellationToken token)
        {
            var start = _time.Now;
            var response = await TryGet(client, endPoint, token);
            return (response.value, start, response.end);
        }

        private async Task<(HttpResponseMessage value, DateTimeOffset end)> TryGet(HttpClient client, string endPoint, CancellationToken token)
        {
            try
            {
                return (await client.GetAsync($"https://{_settings.IP}{endPoint}", token), _time.Now);
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Couldn't fetch from Tesla powerwall");
                return (new HttpResponseMessage(), _time.Now);
            }
        }

        public void Destroy(CancellationToken cancellationToken)
        {
            _client.Destroy(cancellationToken);
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
