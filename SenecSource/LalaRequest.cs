using Newtonsoft.Json;
using SenecEntities;
using Serilog;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly ISenecSettings _senecSettings;

        public LalaRequest(
            ITimeProvider time,
            ISenecSettings senecSettings,
            ILogger logger)
        {
            _time = time;
            _logger = logger;
            _senecSettings = senecSettings;
        }

        public string Content { get; set; }

        public async Task<(string response, DateTimeOffset start, DateTimeOffset end)> Request(CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                using (var response = await GetResponse(client, token))
                {
                    if (IsOk(response.response))
                    {
                        var result = await response.response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(result))
                            _logger.Error("SENEC returned success status code, but {IsNull} data '{result}'", result == null, result);
                        return (result, response.start, response.end);
                    }
                    return (null, response.start, response.end);
                }
            }
        }

        public async Task<TResponse> Request<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            var response = await Request(token);
            var result = Activator.CreateInstance<TResponse>();
            if (string.IsNullOrWhiteSpace(response.response))
            {
                _logger.Warning("SENEC bad response {Response} {Start} {End}", response.response, response.start, response.end);
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
            result.Sent = response.start.ToUnixTimeMilliseconds();
            result.Received = response.end.ToUnixTimeMilliseconds();
            return result;
        }

        public async Task<TResponse> RequestDirectToObject<TResponse>(CancellationToken token) where TResponse : WebResponse
        {
            using (var client = new HttpClient())
            {
                var timeSent = _time.Now;
                using (var response = await GetResponse(client, token))
                {
                    if (IsOk(response.response))
                    {
                        var timeReceived = _time.Now;
                        using (var stream = await response.response.Content.ReadAsStreamAsync())
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

        private bool IsOk(HttpResponseMessage response)
        {
            if (response.Content != null && response.IsSuccessStatusCode)
                return true;

            _logger.Warning("SENEC response was not ok {StatusCode} {@Response}", response.StatusCode, response);
            return false;
        }

        private async Task<ResponseTime> GetResponse(HttpClient client, CancellationToken token)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,de;q=0.8");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            var httpContent = new StringContent(Content, Encoding.UTF8, "application/json");
            var start = DateTimeOffset.Now;
            HttpResponseMessage result = null;
            DateTimeOffset end;
            try
            {
                result = await client.PostAsync($"http://{_senecSettings.IP}/lala.cgi", httpContent, token);
            }
            catch (HttpRequestException e)
            {
                // todo remove ?
                _logger.Warning(e, "Couldn't fetch from SENEC");
                result = new HttpResponseMessage();
            }
            finally
            {
                end = DateTimeOffset.Now;
            }
            return (result, start, end);
        }
    }

    internal struct ResponseTime : IDisposable
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
