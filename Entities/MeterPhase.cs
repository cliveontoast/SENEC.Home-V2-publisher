namespace Entities
{
    public class MeterPhase
    {
        public SenecDecimal Voltage { get; set; }
        public SenecDecimal Current { get; set; }
        public SenecDecimal Power { get; set; }

        public MeterPhase(SenecDecimal power, SenecDecimal current, SenecDecimal voltage)
        {
            Power = power;
            Current = current;
            Voltage = voltage;
        }
    }
}


