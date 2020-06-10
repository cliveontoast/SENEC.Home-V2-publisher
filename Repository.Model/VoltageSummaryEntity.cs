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
            // TODO this MM is minutes. not months, the partition cannot be changed, ever. without destroying the document and re-creating it.
            // TODO stop using the EF Core to connect to cosmosDB, it seems to be... not optimal, i.e. right now I can't read via EF core.. so inserts only :/
            Partition = IntervalStartIncluded.ToString("yyyyMM");
        }

        public string Key { get; set; }
        public string Partition { get; set; }

        // TODO APPLICATION VERSION 1
        public int? Version { get; set; }
        int IRepositoryEntity.Version { get => Version ?? 0; set => Version = value; }
    }
}
