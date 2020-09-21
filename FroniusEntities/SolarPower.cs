using MediatR;
using SenecEntities;
using System;
using System.Net;

namespace FroniusEntities
{
    public class SolarPower : SenecEntities.WebResponse, IRealTimeNotification, INotification
    {
        public DateTimeOffset SourceTimestamp { get; set; }
        public int PowerWatts { get; set; }

        public long ReceivedUnixMillisecondsTimestamp => ReceivedMilliseconds;
        public object SerializableEntity => PowerWatts;
        public bool IsValid => true;

        public SolarPower(long sentMilliseconds, long receivedMilliseconds, DateTimeOffset sourceTimestamp, int powerWatts)
            : base(sentMilliseconds, receivedMilliseconds)
        {
            SourceTimestamp = sourceTimestamp;
            PowerWatts = powerWatts;
        }
    }
}