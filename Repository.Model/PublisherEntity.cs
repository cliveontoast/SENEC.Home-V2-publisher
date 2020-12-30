using Entities;
using Newtonsoft.Json;

namespace Repository.Model
{
    public class PublisherEntity : Publisher, IRepositoryEntity
    {
        public const string DISCRIMINATOR = "Publisher";
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        // required for documentDB
        public PublisherEntity() : base("", default)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public PublisherEntity(Publisher entity, int version) : base(
            entity.Name,
            entity.LastActive)
        {
            Id = PartitionText.PU_ + this.GetKey();
            Partition = PartitionText.PU_.ToString();
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
