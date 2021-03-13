namespace TeslaPowerwallSource
{
    public class TeslaPowerwallSettings : ITeslaPowerwallSettings
    {
        public string? IP { get; set; }
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
