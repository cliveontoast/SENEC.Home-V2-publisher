using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;

namespace TeslaPowerwallSource
{
    internal class FakeApi : IApiRequest
    {
        public async Task<MetersAggregates> GetMetersAggregatesAsync(CancellationToken token)
        {
            var result = await Task.FromResult(JsonConvert.DeserializeObject<MetersAggregates>(@"{""site"":{""last_communication_time"":""2021-03-08T13:41:14.690589661+08:00"",""instant_power"":-1826,""instant_reactive_power"":-734,""instant_apparent_power"":1968.0020325192756,""frequency"":0,""energy_exported"":27775.719544001044,""energy_imported"":1602.4129477757488,""instant_average_voltage"":428.8211389379026,""instant_total_current"":8.167,""i_a_current"":0,""i_b_current"":0,""i_c_current"":0,""last_phase_voltage_communication_time"":""0001-01-01T00:00:00Z"",""last_phase_power_communication_time"":""0001-01-01T00:00:00Z"",""timeout"":1500000000},""battery"":{""last_communication_time"":""2021-03-08T13:41:14.690466542+08:00"",""instant_power"":0,""instant_reactive_power"":360,""instant_apparent_power"":360,""frequency"":49.998,""energy_exported"":41490,""energy_imported"":60110,""instant_average_voltage"":248,""instant_total_current"":-0.7000000000000001,""i_a_current"":0,""i_b_current"":0,""i_c_current"":0,""last_phase_voltage_communication_time"":""0001-01-01T00:00:00Z"",""last_phase_power_communication_time"":""0001-01-01T00:00:00Z"",""timeout"":1500000000},""load"":{""last_communication_time"":""2021-03-08T13:41:14.690308175+08:00"",""instant_power"":99.75,""instant_reactive_power"":-366,""instant_apparent_power"":379.3495255038551,""frequency"":0,""energy_exported"":0,""energy_imported"":69784.23164509439,""instant_average_voltage"":428.8211389379026,""instant_total_current"":0.23261446543204287,""i_a_current"":0,""i_b_current"":0,""i_c_current"":0,""last_phase_voltage_communication_time"":""0001-01-01T00:00:00Z"",""last_phase_power_communication_time"":""0001-01-01T00:00:00Z"",""timeout"":1500000000},""solar"":{""last_communication_time"":""2021-03-08T13:41:14.690308175+08:00"",""instant_power"":1934,""instant_reactive_power"":7,""instant_apparent_power"":1934.0126680040128,""frequency"":0,""energy_exported"":114798.02275267852,""energy_imported"":220.48451135883533,""instant_average_voltage"":427.6260238806801,""instant_total_current"":7.9505,""i_a_current"":0,""i_b_current"":0,""i_c_current"":0,""last_phase_voltage_communication_time"":""0001-01-01T00:00:00Z"",""last_phase_power_communication_time"":""0001-01-01T00:00:00Z"",""timeout"":1500000000}}"));
            result.ReceivedMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            result.SentMilliseconds = result.ReceivedMilliseconds;
            return result;
        }

        public async Task<StateOfEnergy> GetStateOfEnergyAsync(CancellationToken token)
        {
            var result = await Task.FromResult(JsonConvert.DeserializeObject<StateOfEnergy>(@"{""percentage"":69.1675560298826}"));

            result.ReceivedMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            result.SentMilliseconds = result.ReceivedMilliseconds;
            return result;
        }
    }
}
