using System.Collections.Generic;

namespace SenecEntities
{
    public class Meter
    {
        public string FREQ { get; set; }
        public List<string> U_AC { get; set; } // L1 L2 L3
        public List<string> I_AC { get; set; } // L1 L2 L3
        public List<string> P_AC { get; set; } // L1 L2 L3
        public string P_TOTAL { get; set; }
    }
}
