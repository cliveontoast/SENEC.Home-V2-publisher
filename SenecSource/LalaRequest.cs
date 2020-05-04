using Newtonsoft.Json;
using SenecEntities;
using Serilog;
using Shared;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SenecSource
{
    public class LalaRequest : ILalaRequest
    {
        private readonly ITimeProvider _time;
        private readonly ILogger _logger;

        public LalaRequest(
            ITimeProvider time,
            ILogger logger)
        {
            _time = time;
            _logger = logger;
        }

        public string Content { get; set; }

        public async Task<string> Request(CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                using (var response = await GetResponse(client, token))
                {
                    if (IsOk(response))
                        return await response.Content.ReadAsStringAsync();
                }
            }
            return null;
        }

        public async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            var response = await Request(token);
            try
            {
                if (response != null)
                    return JsonConvert.DeserializeObject<TResponse>(response);
            }
            catch (Exception e)
            {
                _logger.Information(e, "Cannot deserialise {Content}", response);
            }
            return null;
        }

        public async Task<TResponse> RequestDirectToObject<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            using (var client = new HttpClient())
            {
                var timeSent = _time.Now;
                using (var response = await GetResponse(client, token))
                {
                    if (IsOk(response))
                    {
                        var timeReceived = _time.Now;
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var sr = new StreamReader(stream))
                        using (var reader = new JsonTextReader(sr))
                        {
                            var serializer = new JsonSerializer();
                            var result = serializer.Deserialize<TResponse>(reader);
                            result.Received = timeReceived.ToUnixTimeMilliseconds();
                            result.Sent = timeSent.ToUnixTimeMilliseconds();
                            return result;
                        }
                    }
                }
            }
            return default;
        }

        private static bool IsOk(HttpResponseMessage response)
        {
            return response.Content != null && response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        private async Task<HttpResponseMessage> GetResponse(HttpClient client, CancellationToken token)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,de;q=0.8");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            var httpContent = new StringContent(Content, Encoding.UTF8, "application/json");
            try
            {
                return await client.PostAsync("http://192.168.0.199/lala.cgi", httpContent, token);
            }
            catch (HttpRequestException e)
            {
                // todo remove ?
                _logger.Warning(e, "Couldn't fetch from SENEC");
                return new HttpResponseMessage();
            }
        }
    }
}
