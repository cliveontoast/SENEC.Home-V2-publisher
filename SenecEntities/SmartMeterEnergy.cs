using MediatR;

namespace SenecEntities
{
    public class SmartMeterEnergy : WebResponse, INotification, IRealTimeNotification
    {
        public SmartMeterEnergy(
            RealTimeClock sourceTimestamp,
            Energy energy,
            long sentMilliseconds,
            long receivedMilliseconds)
            :base(
                 sentMilliseconds,
                 receivedMilliseconds)
        {
            SourceTimestamp = sourceTimestamp;
            Energy = energy;
        }

        public RealTimeClock SourceTimestamp { get; set; }
        public Energy Energy { get; set; }
        public object SerializableEntity => Energy;
        public bool IsValid => SourceTimestamp != null && Energy != null;
        public long ReceivedUnixMillisecondsTimestamp => ReceivedMilliseconds;
    }
}
