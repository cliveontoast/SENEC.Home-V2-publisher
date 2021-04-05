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
        private readonly IAppCache _appCache;

        public ApiRequest(
            ITimeProvider time,
            ILogger logger,
            IAppCache appCache,
            ITeslaPowerwallSettings settings)
        {
            _time = time;
            _logger = logger;
            _settings = settings;
            _appCache = appCache;
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
            else if (type.Equals(typeof(BasicAuthResponse)))
                return "/api/login/Basic";
            throw new NotImplementedException(type.FullName);
        }

        private async Task<(string? response, DateTimeOffset start, DateTimeOffset end)> Request(string endPoint, CancellationToken token)
        {
            using var client = new HttpClient();
            using var response = await GetResponse(client, endPoint, token);
                
            if (IsOk(response.response))
            {
                var result = await response.response.Content.ReadAsStringAsync();
                _logger.Debug(result);
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
            //LocalStore(result);
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
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.5");
            client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("Host", _settings.IP);
            client.DefaultRequestHeaders.Add("Referer", $"https://{_settings.IP}/");
            client.DefaultRequestHeaders.Add("Sec-GPC", "1");
            client.DefaultRequestHeaders.Add("TE", "Trailers");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0");
            client.DefaultRequestHeaders.Add("Cookie", await GetAuthCookies(client, token));
            var start = _time.Now;
            var lalaResponse = await TryGet(client, endPoint, token);
            return (lalaResponse.value, start, lalaResponse.end);
        }

        private async Task<string> GetAuthCookies(HttpClient client, CancellationToken token)
        {
            var authToken = await GetAuthHeaders(client, token);
            var result = string.Join("; ", authToken);
            _logger.Debug("auth cookie text {CookieContent}", result);
            return result;
        }

        private async Task<string[]> GetAuthHeaders(HttpClient client, CancellationToken token)
        {
            var seconds = Math.Abs(_settings.CredentialCacheSeconds);
            var cache = TimeSpan.FromSeconds(Math.Max(60, seconds));
            var result = await _appCache.GetOrAddAsync("teslaAuth", async () =>
            {
                _logger.Information("Fetching auth cookies");
                if (string.IsNullOrWhiteSpace(_settings.IP))
                {
                    var txt = $"Setting is unspecified '{_settings.IP}'";
                    _logger.Fatal(txt);
                    throw new Exception(txt);
                }
                if (string.IsNullOrWhiteSpace(_settings.Password))
                {
                    var txt = $"Setting is unspecified '{_settings.Password}'";
                    _logger.Fatal(txt);
                    throw new Exception(txt);
                }
                try
                {
                    var requestContent = JsonConvert.SerializeObject(new BasicAuthRequestContent { email = _settings.Email, password = _settings.Password });
                    var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/json");
                    var endPoint = GetEndPoint<BasicAuthResponse>();
                    string requestUri = $"https://{_settings.IP}{endPoint}";
                    _logger.Information("Auth sending to {Http} Content: {Content}", requestUri, requestContent);
                    var response = await client.PostAsync(requestUri, httpContent, token);
                    if (!IsOk(response))
                        throw new InvalidOperationException($"Not ok {response.StatusCode}");
                    //var responseContent = await response.Content.ReadAsStreamAsync();
                    if (!response.Headers.TryGetValues("Set-Cookie", out var authCookies))
                        throw new KeyNotFoundException("Set-Cookie");
                    _logger.Information("Auth response ok {@Cookie}", authCookies);
                    return authCookies.Select(a => a.Replace("; Path=/", "")).ToArray();
                }
                catch (Exception e)
                {
                    throw;
                }
            }, cache);
            return result;
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
