using MediatR;

namespace SenecEntities
{
    public class GridMeter : WebResponse, INotification, IRealTimeNotification
    {
        public Meter Meter { get; set; }
        public RealTimeClock SourceTimestamp { get; set; }

        public GridMeter(RealTimeClock sourceTimestamp, Meter meter, long sentMilliseconds, long receivedMilliseconds): base(sentMilliseconds, receivedMilliseconds)
        {
            Meter = meter;
            SourceTimestamp = sourceTimestamp;
        }

        public object SerializableEntity => Meter;
        public bool IsValid => SourceTimestamp != null && Meter != null;
        public long ReceivedUnixMillisecondsTimestamp => ReceivedMilliseconds;
    }
}
