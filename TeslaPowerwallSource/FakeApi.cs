﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaEntities;

namespace TeslaPowerwallSource
{
    public class FakeApi : IApiRequest
    {
        public async Task<MetersAggregates> GetMetersAggregatesAsync(CancellationToken token)
        {
            var result = await Task.FromResult(JsonConvert.DeserializeObject<MetersAggregates>(@"{
   ""site"":{
      ""last_communication_time"":""2018-04-02T16:11:41.885377469-07:00"",
      ""instant_power"":-21.449996948242188,
      ""instant_reactive_power"":-138.8300018310547,
      ""instant_apparent_power"":140.47729986545957,
      ""frequency"":60.060001373291016,
      ""energy_exported"":1136916.6875890202,
      ""energy_imported"":3276432.6625890196,
      ""instant_average_voltage"":239.81999969482422,
      ""instant_total_current"":0,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   },
   ""battery"":{
      ""last_communication_time"":""2018-04-02T16:11:41.89022247-07:00"",
      ""instant_power"":-2350,
      ""instant_reactive_power"":0,
      ""instant_apparent_power"":2350,
      ""frequency"":60.033,
      ""energy_exported"":1169030,
      ""energy_imported"":1638140,
      ""instant_average_voltage"":239.10000000000002,
      ""instant_total_current"":45.8,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   },
   ""load"":{
      ""last_communication_time"":""2018-04-02T16:11:41.885377469-07:00"",
      ""instant_power"":1546.2712597712405,
      ""instant_reactive_power"":-71.43153973801415,
      ""instant_apparent_power"":1547.920305979569,
      ""frequency"":60.060001373291016,
      ""energy_exported"":0,
      ""energy_imported"":7191016.994444443,
      ""instant_average_voltage"":239.81999969482422,
      ""instant_total_current"":6.44763264839839,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   },
   ""solar"":{
      ""last_communication_time"":""2018-04-02T16:11:41.885541803-07:00"",
      ""instant_power"":3906.1700439453125,
      ""instant_reactive_power"":53.26999855041504,
      ""instant_apparent_power"":3906.533259164868,
      ""frequency"":60.060001373291016,
      ""energy_exported"":5534272.949724403,
      ""energy_imported"":13661.930279959455,
      ""instant_average_voltage"":239.8699951171875,
      ""instant_total_current"":0,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   },
   ""busway"":{
      ""last_communication_time"":""0001-01-01T00:00:00Z"",
      ""instant_power"":0,
      ""instant_reactive_power"":0,
      ""instant_apparent_power"":0,
      ""frequency"":0,
      ""energy_exported"":0,
      ""energy_imported"":0,
      ""instant_average_voltage"":0,
      ""instant_total_current"":0,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   },
   ""frequency"":{
      ""last_communication_time"":""0001-01-01T00:00:00Z"",
      ""instant_power"":0,
      ""instant_reactive_power"":0,
      ""instant_apparent_power"":0,
      ""frequency"":0,
      ""energy_exported"":0,
      ""energy_imported"":0,
      ""instant_average_voltage"":0,
      ""instant_total_current"":0,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   },
   ""generator"":{
      ""last_communication_time"":""0001-01-01T00:00:00Z"",
      ""instant_power"":0,
      ""instant_reactive_power"":0,
      ""instant_apparent_power"":0,
      ""frequency"":0,
      ""energy_exported"":0,
      ""energy_imported"":0,
      ""instant_average_voltage"":0,
      ""instant_total_current"":0,
      ""i_a_current"":0,
      ""i_b_current"":0,
      ""i_c_current"":0
   }
}"));
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
