namespace SenecSource
{
    public interface ILalaRequestBuilder
    {
        ILalaRequestBuilder AddGridMeter();
        ILalaRequest Build();
        ILalaRequestBuilder AddTime();
    }
}