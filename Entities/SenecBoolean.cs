﻿using System.Diagnostics;

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

        public bool IsTrue => Value.HasValue && Value.Value > 0;
        public bool IsFalse => Value.HasValue && Value.Value == 0;
        public bool IsNull => !Value.HasValue;
    }
}
