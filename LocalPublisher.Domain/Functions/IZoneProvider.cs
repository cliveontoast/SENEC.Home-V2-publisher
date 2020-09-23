namespace Domain
{
    public interface IZoneProvider
    {
        string NodaTimeZone { get; }
    }

    public class ZoneProvider : IZoneProvider
    {
        public string NodaTimeZone { get; }

        public ZoneProvider(string nodaTimeZone)
        {
            NodaTimeZone = nodaTimeZone;
        }
    }
}
