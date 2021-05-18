using Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Model
{
    public class IntervalOfMomentsEntity<TMoment> : IntervalOfMoments<TMoment>, IRepositoryEntity where TMoment : IIsValid
    {
        public const string DISCRIMINATOR = "IntervalOfMoments";
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        // required for documentDB
        public IntervalOfMomentsEntity() : base(default, default, default)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public IntervalOfMomentsEntity(IIntervalEntity entity, int version, IEnumerable<object> list, Func<object, TMoment> func) : this(
            new IntervalOfMoments<TMoment>(entity.IntervalStartIncluded, entity.IntervalEndExcluded, list.Select(a => func(a))), version)
        {
        }

        public IntervalOfMomentsEntity(IntervalOfMoments<TMoment> entity, int version) : base(
            intervalEndExcluded: entity.IntervalEndExcluded,
            intervalStartIncluded: entity.IntervalStartIncluded,
            moments: entity.Moments)
        {
            Id = GetPartitionText<TMoment>() + GetRepositoryKey(IntervalStartIncluded);
            Partition = Id;
            Version = version;
        }

        public static string GetRepositoryKey(DateTimeOffset date)
        {
            return $"{date.DayOfWeek}{date.TimeOfDay}";
        }

        private string GetPartitionText<T>()
        {
            // TODO
            if (typeof(T) == typeof(VoltageMomentEntity))
            {
                return PartitionText.MV_.ToString();
            }
            throw new NotImplementedException(typeof(T).Name);
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
