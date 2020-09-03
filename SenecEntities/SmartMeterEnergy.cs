using MediatR;

namespace SenecEntities
{
    public class SmartMeterEnergy : WebResponse, INotification, IRealTimeNotification
    {
        public RealTimeClock RTC { get; set; }
        public Energy ENERGY { get; set; }
        public object SerializableEntity => ENERGY;
    }
}
