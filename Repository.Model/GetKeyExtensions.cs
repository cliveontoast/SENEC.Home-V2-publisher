using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Repository
{
    public static class GetKeyExtensions
    {
        public static string GetKey(this Entities.EnergySummary voltageSummary)
        {
            // TODO cosmosdb 'Items' really one giant bucket?
            return GetIntervalKey(voltageSummary.IntervalStartIncluded);
        }
        public static string GetKey(this Entities.VoltageSummary voltageSummary)
        {
            return GetIntervalKey(voltageSummary.IntervalStartIncluded);
        }
        public static string GetIntervalKey(this DateTimeOffset intervalStart)
        {
            return JsonConvert.SerializeObject(intervalStart).Replace(@"""", "");
        }
    }
}
