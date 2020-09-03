using System.Runtime.Serialization;

namespace SenecEntities
{
    public interface IRealTimeNotification
    {
        RealTimeClock RTC { get; set; }
        object SerializableEntity { get; }
    }
}