using MediatR;
using System.Runtime.Serialization;

namespace SenecEntities
{
    public class GridMeter : WebResponse, INotification, IRealTimeNotification
    {
        public Meter PM1OBJ1 { get; set; }
        public RealTimeClock RTC { get; set; }

        public GridMeter(RealTimeClock rTC, Meter pM1OBJ1, long sent, long received): base(sent, received)
        {
            RTC = rTC;
            PM1OBJ1 = pM1OBJ1;
        }

        public object SerializableEntity => PM1OBJ1;
    }
}
