using MediatR;

namespace SenecEntities
{
    public class SmartMeterEnergy : WebResponse, INotification, IRealTimeNotification
    {
        public SmartMeterEnergy(
            RealTimeClock rTC,
            Energy eNERGY,
            long sent,
            long received)
            :base(
                 sent,
                 received)
        {
            RTC = rTC;
            ENERGY = eNERGY;
        }

        public RealTimeClock RTC { get; set; }
        public Energy ENERGY { get; set; }
        public object SerializableEntity => ENERGY;
    }
}
