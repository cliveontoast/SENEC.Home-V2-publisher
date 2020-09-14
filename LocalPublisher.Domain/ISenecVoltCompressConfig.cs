namespace Domain
{
    public interface ISenecVoltCompressConfig
    {
        int MinutesPerSummary { get; }
    }
    public interface ISenecEnergyCompressConfig
    {
        int MinutesPerSummary { get; }
    }

    public class SenecCompressConfig : ISenecVoltCompressConfig, ISenecEnergyCompressConfig
    {
        public int MinutesPerSummary { get; set; }
        public int? PersistedVersion { get; set; }
    }
}
