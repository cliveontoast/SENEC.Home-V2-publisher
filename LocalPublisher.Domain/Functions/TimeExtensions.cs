using LocalPublisher.Domain.Functions;
using NodaTime;
using System;

namespace Domain
{
    public static class TimeExtensions
    {
        public static DateTimeOffset ToEquipmentLocalTime(this long unixSeconds, IZoneProvider timezone)
        {
            return ToEquipmentLocalTime(DateTimeOffset.FromUnixTimeSeconds(unixSeconds), timezone);
        }
        public static DateTimeOffset ToEquipmentLocalTime(this DateTimeOffset utcDateTime, IZoneProvider timezone)
        {
            var dateTimeAtSite = ZonedDateTime.FromDateTimeOffset(utcDateTime).WithZone(timezone.DateTimeZone);
            return dateTimeAtSite.ToDateTimeOffset();
        }
    }
}
