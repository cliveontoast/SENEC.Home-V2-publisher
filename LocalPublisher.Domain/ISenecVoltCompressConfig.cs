namespace Domain
{
    public interface IMinutesPerSummaryConfig
    {
        int MinutesPerSummary { get; }
    }
    public interface ISenecVoltCompressConfig: IMinutesPerSummaryConfig
    {
    }
    public interface ISenecEnergyCompressConfig: IMinutesPerSummaryConfig
    {
    }
    public interface ISenecBatteryInverterTemperatureCompressConfig: IMinutesPerSummaryConfig
    {
    }

    public class SenecCompressConfig : 
        ISenecVoltCompressConfig,
        ISenecEnergyCompressConfig,
        ISenecBatteryInverterTemperatureCompressConfig
    {
        public int MinutesPerSummary { get; set; }
        public int? PersistedVersion { get; set; }
    }
}
