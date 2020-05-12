using Newtonsoft.Json;
using System;

namespace Repository.Model
{
    public class VoltageSummary : Entities.VoltageSummary
    {
        // EF
        public VoltageSummary()
        {
        }

        public VoltageSummary(Entities.VoltageSummary entity, int version)
        {
            // automapper
            IntervalEndExcluded = entity.IntervalEndExcluded;
            IntervalStartIncluded = entity.IntervalStartIncluded;
            L1 = entity.L1;
            L2 = entity.L2;
            L3 = entity.L3;
            Version = version;
        }

        public string Key
        {
            get => this.GetKey();
            set => IntervalStartIncluded = JsonConvert.DeserializeObject<DateTimeOffset>($@"""{value}""");
        }
        public string Partition => IntervalStartIncluded.ToString("yyyymm");

        // TODO APPLICATION VERSION 1
        public int? Version { get; set; }
    }
}
