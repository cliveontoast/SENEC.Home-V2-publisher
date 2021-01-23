using Newtonsoft.Json;
using System;

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
        public static string GetKey(this Entities.EquipmentStatesSummary voltageSummary)
        {
            return GetIntervalKey(voltageSummary.IntervalStartIncluded);
        }
        public static string GetKey(this Entities.Publisher voltageSummary)
        {
            return voltageSummary.Name;
        }
        public static string GetKey<TMoment>(this Entities.IntervalOfMoments<TMoment> voltageSummary) where TMoment : Entities.IIsValid
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
        public static string GetKeyVersion2(this Entities.EquipmentStatesSummary voltageSummary)
        {
            return GetIntervalKeyVersion2(voltageSummary.IntervalStartIncluded);
        }
        public static string GetKeyVersion2<TMoment>(this Entities.IntervalOfMoments<TMoment> voltageSummary) where TMoment : Entities.IIsValid
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
