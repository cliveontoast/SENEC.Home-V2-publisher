namespace SenecEntities
{
    public class WebResponse
    {
        public long Sent { get; set; }
        public long Received { get; set; }

        public WebResponse(long sent, long received)
        {
            Sent = sent;
            Received = received;
        }
    }
}
