using NodaTime;

namespace Domain
{
    public interface IZoneProvider
    {
        string NodaTimeZone { get; }
        DateTimeZone DateTimeZone { get; }
    }

    public class ZoneProvider : IZoneProvider
    {
        public string NodaTimeZone { get; }

        public DateTimeZone DateTimeZone { get; }

        public ZoneProvider(string nodaTimeZone)
        {
            NodaTimeZone = nodaTimeZone;
            DateTimeZone = DateTimeZoneProviders.Tzdb[NodaTimeZone];
        }
    }
}
