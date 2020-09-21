namespace SenecEntities
{
    public abstract class WebResponse
    {
        public long SentMilliseconds { get; set; }
        public long ReceivedMilliseconds { get; set; }

        public WebResponse(long sentMilliseconds, long receivedMilliseconds)
        {
            SentMilliseconds = sentMilliseconds;
            ReceivedMilliseconds = receivedMilliseconds;
        }
    }
}
