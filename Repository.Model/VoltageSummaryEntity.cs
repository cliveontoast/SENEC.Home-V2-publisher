namespace Repository.Model
{
    public class VoltageSummary : Entities.VoltageSummary, IRepositoryEntity
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
            Key = entity.GetKey();
            Partition = IntervalStartIncluded.ToString("yyyymm");
        }

        public string Key { get; set; }
        public string Partition { get; set; }

        // TODO APPLICATION VERSION 1
        public int? Version { get; set; }
        int IRepositoryEntity.Version { get => Version ?? 0; set => Version = value; }
    }
}
