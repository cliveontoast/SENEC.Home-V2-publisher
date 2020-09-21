using System.Runtime.Serialization;

namespace SenecEntities
{
    public interface IRealTimeNotification
    {
        long ReceivedUnixMillisecondsTimestamp { get; }
        object SerializableEntity { get; }
        bool IsValid { get; }
    }
}