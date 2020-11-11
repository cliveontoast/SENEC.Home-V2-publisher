using MediatR;

namespace SenecEntities
{
    public class BatteryInverterTemperature : WebResponse, INotification, IRealTimeNotification
    {
        public BatteryObject1 Inverter { get; set; }
        public RealTimeClock SourceTimestamp { get; set; }

        public BatteryInverterTemperature(RealTimeClock sourceTimestamp, BatteryObject1 inverter, long sentMilliseconds, long receivedMilliseconds): base(sentMilliseconds, receivedMilliseconds)
        {
            Inverter = inverter;
            SourceTimestamp = sourceTimestamp;
        }

        public object SerializableEntity => Inverter;
        public bool IsValid => SourceTimestamp != null && Inverter != null;
        public long ReceivedUnixMillisecondsTimestamp => ReceivedMilliseconds;
    }
}
