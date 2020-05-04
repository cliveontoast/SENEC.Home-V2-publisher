namespace Entities
{
    [System.Diagnostics.DebuggerDisplay("{ToString()} {TypeString}")]
    public class SenecString : SenecValue
    {
        public const int Character = 9;
        public const int String = 10;
        public const int Error = 11;
        public const int Unknown = 12;

        public static implicit operator string(SenecString s) => s.Value;

        public SenecString(int type, string value)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; private set; }
        public int Type { get; set; }

        private string TypeString
        {
            get
            {
                switch (Type)
                {
                    case Character: return nameof(Character);
                    case String: return nameof(String);
                    case Error: return nameof(Error);
                    case Unknown: return nameof(Unknown);
                    default: return "Damn programmers not doing their job";
                }
            }
        }

        public override string ToString()
        {
            if (Value == null) return "undefined";
            return Value;
        }
    }
}
