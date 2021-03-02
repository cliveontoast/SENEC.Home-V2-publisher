using MediatR;
using SenecEntities;
using System;

namespace TeslaEntities
{
    public class MetersAggregatesItem : INotification, IRealTimeNotification
    {
        public MetersAggregatesItem(
            MetersAggregates item)
        {
            Item = item;
        }

        public object SerializableEntity => Item!;
        public bool IsValid => Item.ReceivedMilliseconds > 0;
        public long ReceivedUnixMillisecondsTimestamp => Item.ReceivedMilliseconds;

        public MetersAggregates Item { get; }
    }
}
