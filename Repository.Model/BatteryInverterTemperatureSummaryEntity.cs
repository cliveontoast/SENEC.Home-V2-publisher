using Entities;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Repository.Model
{
    public class BatteryInverterTemperatureSummaryEntity : InverterTemperatureSummary, IRepositoryEntity
    {
        public const string DISCRIMINATOR = "InverterTemperature";
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        // required for documentDB
        public BatteryInverterTemperatureSummaryEntity() : base(default, default, default, new decimal?[0])
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public BatteryInverterTemperatureSummaryEntity(InverterTemperatureSummary entity, int version) : base(
            intervalEndExcluded: entity.IntervalEndExcluded,
            intervalStartIncluded: entity.IntervalStartIncluded,
            temperatures: entity.Temperatures.ToArray(),
            secondsWithoutData:entity.SecondsWithoutData)
        {
            Id = this.GetKey();
            Partition = "ITS_" + this.GetKey();
            Version = version;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Partition { get; set; }
        public string Discriminator { get => DISCRIMINATOR; }
        public int Version { get; set; }
        string IRepositoryEntity.Key { get => Id; set => Id = value; }
    }
}
