using System;

namespace TeslaEntities
{
    public class StateOfEnergy : WebResponse
    {
        public StateOfEnergy() : base(default, default)
        {
        }

        // TODO is it ever null?
        public decimal? percentage { get; set; }
    }
}
