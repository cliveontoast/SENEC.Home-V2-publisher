using MediatR;

namespace SenecEntities
{
    public class GridMeter : WebResponse, INotification
    {
        public Meter PM1OBJ1 { get; set; }
        public WebTime RTC { get; set; }
    }
}
