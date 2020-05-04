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

        public VoltageSummary(Entities.VoltageSummary entity)
        {
            // automapper
            IntervalEndExcluded = entity.IntervalEndExcluded;
            IntervalStartIncluded = entity.IntervalStartIncluded;
            L1 = entity.L1;
            L2 = entity.L2;
            L3 = entity.L3;
        }

        public string Key
        {
            get => JsonConvert.SerializeObject(IntervalStartIncluded).Replace(@"""", "");
            set => IntervalStartIncluded = JsonConvert.DeserializeObject<DateTimeOffset>($@"""{value}""");
        }
        public string Partition => IntervalStartIncluded.ToString("yyyymm");
    }
}
