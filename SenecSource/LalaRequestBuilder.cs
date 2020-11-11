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
            result = new JObject();
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
            result.Add(nameof(LalaResponseContent.RTC), JObject.FromObject(new RealTimeClock()));
            return this;
        }

        public ILalaRequestBuilder AddStatistics()
        {
            result.Add(nameof(LalaResponseContent.STATISTIC), JObject.FromObject(new Statistic()));
            return this;
        }

        public ILalaRequestBuilder AddEnergy()
        {
            result.Add(nameof(LalaResponseContent.ENERGY), JObject.FromObject(new Energy()));
            return this;
        }

        public ILalaRequestBuilder AddInverterTemperature()
        {
            result.Add(nameof(LalaResponseContent.BAT1OBJ1), JObject.FromObject(new BatteryObject1()));
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
