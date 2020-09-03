using MediatR;
using System.Runtime.Serialization;

namespace SenecEntities
{
    public class GridMeter : WebResponse, INotification, IRealTimeNotification
    {
        public Meter PM1OBJ1 { get; set; }
        public RealTimeClock RTC { get; set; }
        public object SerializableEntity => PM1OBJ1;
    }
}
