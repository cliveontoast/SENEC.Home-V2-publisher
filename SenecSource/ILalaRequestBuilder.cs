namespace SenecSource
{
    public interface ILalaRequestBuilder
    {
        ILalaRequestBuilder AddGridMeter();
        ILalaRequest Build();
        ILalaRequestBuilder AddTime();
        ILalaRequestBuilder AddStatistics();
        ILalaRequestBuilder AddEnergy();
        ILalaRequestBuilder AddInverterTemperature();
    }
}