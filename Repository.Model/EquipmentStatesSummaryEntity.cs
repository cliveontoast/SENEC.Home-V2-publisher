using Entities;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Repository.Model
{
    public class EquipmentStatesSummaryEntity : EquipmentStatesSummary, IRepositoryEntity
    {
        public const string DISCRIMINATOR = "EquipmentStates";
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        // required for documentDB
        public EquipmentStatesSummaryEntity() : base(default, default,  Enumerable.Empty<EquipmentStateStatistic>(), default)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public EquipmentStatesSummaryEntity(EquipmentStatesSummary entity, int version) : base(
            intervalEndExcluded: entity.IntervalEndExcluded,
            intervalStartIncluded: entity.IntervalStartIncluded,
            states : entity.States,
            secondsWithoutData:entity.SecondsWithoutData)
        {
            Id = PartitionText.EQ_ + this.GetKey();
            Partition = PartitionText.EQ_ + this.GetKey();
            Version = version;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Partition { get; set; }
        public string Key { get => Partition; set {; } }
        public string Discriminator { get => DISCRIMINATOR; }
        public int Version { get; set; }
        string IRepositoryEntity.Key { get => Id; set => Id = value; }
    }
}
