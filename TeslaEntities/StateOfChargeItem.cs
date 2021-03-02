using MediatR;
using SenecEntities;

namespace TeslaEntities
{
    public class StateOfEnergyItem : INotification, IRealTimeNotification
    {
        public StateOfEnergyItem(
            StateOfEnergy? item)
        {
            Item = item;
        }

        public object SerializableEntity => Item!;
        public bool IsValid => Item != null;
        public long ReceivedUnixMillisecondsTimestamp => Item == null ? 0 : Item.ReceivedMilliseconds;

        public StateOfEnergy? Item { get; }
    }
}
