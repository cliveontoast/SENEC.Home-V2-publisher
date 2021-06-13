using LazyCache;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeslaPowerwallSource
{
    public class Client : IClient
    {
        public const string LOGGING = "TeslaClient: ";
        private const string CACHE_KEY = "telsaClient";
        private static readonly object __lock = new object();
        private readonly IAppCache _appCache;
        private readonly ILogger _logger;
        private readonly ITeslaPowerwallSettings _settings;
        private readonly IEndPoint _endPoint;

        public Client(IAppCache appCache, ITeslaPowerwallSettings settings, ILogger logger, IEndPoint endPoint)
        {
            _appCache = appCache;
            _settings = settings;
            _logger = logger;
            _endPoint = endPoint;
        }

        public async Task<HttpClient> Build(CancellationToken token)
        {
            Task<HttpClient>? clientTask;
            lock (__lock)
            {
                clientTask = _appCache.GetOrAddAsync(CACHE_KEY, () => GetHttpClient(token), DateTimeOffset.Now.AddMinutes(10));
            }
            var client = await clientTask;
            return client;
        }

        public void Destroy(CancellationToken token)
        {
            lock (__lock)
            {
                _appCache.Remove(CACHE_KEY);
            }
        }

        private async Task<HttpClient> GetHttpClient(CancellationToken token)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(1);
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
            return client;
        }

        private async Task<string> GetAuthCookies(HttpClient client, CancellationToken token)
        {
            var authToken = await GetAuthHeaders(client, token);
            var result = string.Join("; ", authToken);
            _logger.Debug(LOGGING + "auth cookie text {CookieContent}", result);
            return result;
        }

        private async Task<string[]> GetAuthHeaders(HttpClient client, CancellationToken token)
        {
            _logger.Information(LOGGING + "Fetching auth cookies");
            if (string.IsNullOrWhiteSpace(_settings.IP))
            {
                var txt = LOGGING + $"Setting is unspecified '{_settings.IP}'";
                _logger.Fatal(txt);
                throw new Exception(txt);
            }
            if (string.IsNullOrWhiteSpace(_settings.Password))
            {
                var txt = LOGGING + $"Setting is unspecified '{_settings.Password}'";
                _logger.Fatal(txt);
                throw new Exception(txt);
            }
            try
            {
                var requestContent = JsonConvert.SerializeObject(new BasicAuthRequestContent { email = _settings.Email, password = _settings.Password });
                var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/json");
                var endPoint = _endPoint.GetEndPoint<BasicAuthResponse>();
                string requestUri = $"https://{_settings.IP}{endPoint}";
                _logger.Information(LOGGING + "Auth sending to {Http} Content: {Content}", requestUri, requestContent);
                var response = await client.PostAsync(requestUri, httpContent, token);
                if (!IsOk(response))
                    throw new InvalidOperationException($"Not ok {response.StatusCode}");
                if (!response.Headers.TryGetValues("Set-Cookie", out var authCookies))
                    throw new KeyNotFoundException("Set-Cookie");
                _logger.Information(LOGGING + "Auth response ok {@Cookie}", authCookies);
                return authCookies.Select(a => a.Replace("; Path=/", "")).ToArray();
            }
            catch (Exception e)
            {
                _logger.Error(e, LOGGING + $"Auth exception {e.GetType().Name}");
                throw;
            }
        }

        private bool IsOk(HttpResponseMessage response)
        {
            return _endPoint.IsOk(response);
        }
    }

    public interface IClient
    {
        Task<HttpClient> Build(CancellationToken token);
        void Destroy(CancellationToken token);
    }
}
