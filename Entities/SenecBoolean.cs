using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Entities
{
    [DebuggerDisplay("{IsTrue}")]
    public class SenecBoolean : SenecDecimal
    {
        public SenecBoolean(SenecDecimal senecDecimal) : this(senecDecimal.Type, senecDecimal.Value)
        {
        }
        public SenecBoolean(int type, decimal? value = null) : base(type, value)
        {
        }

        public bool IsTrue { get => Value.HasValue && Value.Value > 0; }
        public bool IsFalse { get => Value.HasValue && Value.Value == 0; }
        public bool IsNull { get => !Value.HasValue; }
    }
}
