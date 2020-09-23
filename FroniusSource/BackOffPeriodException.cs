using System;
using System.Net.Http;
using System.Runtime.Serialization;

namespace FroniusSource
{
    [Serializable]
    public class BackOffPeriodException : Exception
    {
        public BackOffPeriodException(HttpRequestException e, TimeSpan newPollingTime) : base("Back off period", e)
        {
            BackoffTime = newPollingTime;
        }

        public TimeSpan BackoffTime { get; }
    }
}