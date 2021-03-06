﻿using Newtonsoft.Json;
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
        public static string GetKey(this Entities.InverterTemperatureSummary voltageSummary)
        {
            return GetIntervalKey(voltageSummary.IntervalStartIncluded);
        }
        public static string GetIntervalKey(this DateTimeOffset intervalStart)
        {
            return JsonConvert.SerializeObject(intervalStart).Replace(@"""", "");
        }

        public static string GetKeyVersion2(this Entities.EnergySummary voltageSummary)
        {
            return GetIntervalKeyVersion2(voltageSummary.IntervalStartIncluded);
        }
        public static string GetKeyVersion2(this Entities.InverterTemperatureSummary voltageSummary)
        {
            return GetIntervalKeyVersion2(voltageSummary.IntervalStartIncluded);
        }
        public static string GetKeyVersion2(this Entities.VoltageSummary voltageSummary)
        {
            // TODO cosmosdb 'Items' really one giant bucket?
            return GetIntervalKeyVersion2(voltageSummary.IntervalStartIncluded);
        }
        public static string GetIntervalKeyVersion2(this DateTimeOffset intervalStart)
        {
            var oldKey = new DateTimeOffset(intervalStart.DateTime, TimeSpan.Zero);
            return JsonConvert.SerializeObject(oldKey).Replace(@"""", "");
        }
    }
}
