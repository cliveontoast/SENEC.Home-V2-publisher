﻿namespace Repository.Model
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
            // TODO All documents written with version 0 used partition of yyyyMM i.e. capital MM == month.
            // need to re-write those documents to the database with the partition of yyyymm <-- old version... and 1st record of Version 1 use this bad partion
            // records up to and including June 16th 2020 require re-writing.
            // stop using the EF Core 3.1 to connect to cosmosDB, it seems to be... not optimal, i.e. right now I can't read via EF core.. so inserts only :/
            // either try EF Core 5 preview, or move to cosmosDB client.
            Partition = IntervalStartIncluded.ToString("yyyyMM");
        }

        public string Key { get; set; }
        public string Partition { get; set; }

        // TODO APPLICATION VERSION 1
        public int? Version { get; set; }
        int IRepositoryEntity.Version { get => Version ?? 0; set => Version = value; }
    }
}
