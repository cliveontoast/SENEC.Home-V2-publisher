using Newtonsoft.Json.Linq;
using SenecEntities;
using System;

namespace SenecSource
{
    public class LalaRequestBuilder : ILalaRequestBuilder
    {
        private JObject result;
        private readonly Func<ILalaRequest> _buildRequest;

        public LalaRequestBuilder(Func<ILalaRequest> buildRequest)
        {
            Restart();
            _buildRequest = buildRequest;
        }

        public ILalaRequest Build()
        {
            var request = _buildRequest();
            request.Content = result.ToString().Replace("null", @"""""");
            return request;
        }

        public ILalaRequestBuilder Restart()
        {
            result = new JObject();
            return this;
        }

        public ILalaRequestBuilder AddGridMeter()
        {
            result.Add(nameof(LalaResponseContent.PM1OBJ1), JObject.FromObject(new Meter()));
            return this;
        }

        public ILalaRequestBuilder AddTime()
        {
            result.Add(nameof(LalaResponseContent.RTC), JObject.FromObject(new WebTime()));
            return this;
        }

        //public async Task<LalaRequestBuilder> AddConsumption()
        //{
        //    return await AddResource("lalaRequest.SmartMeterConsumption.json");
        //}

        //private async Task<LalaRequestBuilder> AddResource(string resource)
        //{
        //    var text = await resource.GetEmbeddedResource();
        //    var json = JObject.Parse(text);
        //    result.Merge(json);
        //    return this;
        //}
    }
}
