namespace Domain
{
    public interface ISenecCompressConfig
    {
        int MinutesPerSummary { get; }
    }

    public class SenecCompressConfig : ISenecCompressConfig
    {
        public int MinutesPerSummary { get; set; }
        public int? PersistedVersion { get; set; }
    }
}
