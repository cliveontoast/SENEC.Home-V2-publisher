namespace TeslaPowerwallSource
{
    public class BasicAuthRequestContent
    {
        public string username { get; set; } = "customer";
        public string password { get; set; } = "last5charactersOfSerial";
        public string email { get; set; } = "tesla@example.com";
        public bool force_sm_off { get; set; } = false;
    }
}
