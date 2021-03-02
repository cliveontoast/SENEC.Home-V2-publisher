using Entities;

namespace SenecEntitiesAdapter
{
    public interface IAdapter
    {
        SenecDecimal GetDecimal(string? data);
        SenecValue GetValue(string? data);
    }
}