using MediatR;

namespace SenecEntities
{
    public class BatteryInverterTemperature : WebResponse, INotification, IRealTimeNotification
    {
        public BatteryObject1 Inverter { get; set; }
        public RealTimeClock SourceTimestamp { get; set; }
        public TemperatureMeasure TemperatureMeasurements { get; private set; }

        public BatteryInverterTemperature(
            RealTimeClock sourceTimestamp,
            BatteryObject1 inverter,
            TemperatureMeasure? other,
            long sentMilliseconds, long receivedMilliseconds) : base(sentMilliseconds, receivedMilliseconds)
        {
            Inverter = inverter;
            SourceTimestamp = sourceTimestamp;
            TemperatureMeasurements = other ?? new TemperatureMeasure();
        }

        public object SerializableEntity => new Temperatures { Inverter = Inverter, TemperatureMeasurements = TemperatureMeasurements };
        public bool IsValid => SourceTimestamp != null && Inverter != null;
        public long ReceivedUnixMillisecondsTimestamp => ReceivedMilliseconds;
    }

    public class Temperatures
    {
        public TemperatureMeasure TemperatureMeasurements { get; set; } = null!;
        public BatteryObject1 Inverter { get; set; } = null!;
    }
}
