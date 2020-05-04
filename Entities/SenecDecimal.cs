namespace Entities
{
    [System.Diagnostics.DebuggerDisplay("{ToString()} {TypeString}")]
    public class SenecDecimal : SenecValue
    {
        public const int Float = 1;
        public const int Unsigned1 = 2;
        public const int Unsigned3 = 3;
        public const int Unsigned6 = 4;
        public const int Unsigned8 = 5;
        public const int Integer1 = 6;
        public const int Integer3 = 7;
        public const int Integer8 = 8; 

        public static implicit operator decimal?(SenecDecimal d) => d.Value;
        public SenecDecimal(int type, decimal? value = null)
        {
            Value = value;
            Type = type;
        }

        public decimal? Value { get; set; }
        public int Type { get; }

        private string TypeString
        {
            get
            {
                switch (Type)
                {
                    case Float: return nameof(Float);
                    case Unsigned1: return nameof(Unsigned1);
                    case Unsigned3: return nameof(Unsigned3);
                    case Unsigned6: return nameof(Unsigned6);
                    case Unsigned8: return nameof(Unsigned8);
                    case Integer1: return nameof(Integer1);
                    case Integer3: return nameof(Integer3);
                    case Integer8: return nameof(Integer8);
                    default: return "Damn programmers not doing their job";
                }
            }
        }

        public override string ToString()
        {
            if (Value == null) return "undefined";
            switch (Type)
            {
                case Unsigned1:
                case Unsigned8:
                case Integer1:
                case Integer3:
                case Integer8:
                    return Value.Value.ToString("00");
                default:
                    return Value.ToString();
            }
        }
    }
}
