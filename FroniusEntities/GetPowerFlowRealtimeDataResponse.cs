using System;
using System.Net;

namespace FroniusEntities
{
    public class Site
    {
        public long? E_Day { get; set; }
        public long? E_Total { get; set; }
        public long? E_Year { get; set; }
        public string? Meter_Location { get; set; }
        public string? Mode { get; set; }
        public object? P_Akku { get; set; }
        public object? P_Grid { get; set; }
        public object? P_Load { get; set; }
        public int? P_PV { get; set; }
        public object? rel_Autonomy { get; set; }
        public object? rel_SelfConsumption { get; set; }
    }

    public class Data
    {
        public Site? Site { get; set; }
        public string? Version { get; set; }
    }

    public class Body
    {
        public Data? Data { get; set; }
    }

    public class RequestArguments
    {
    }

    public class Status
    {
        public int? Code { get; set; }
        public string? Reason { get; set; }
        public string? UserMessage { get; set; }
    }

    public class Head
    {
        public RequestArguments? RequestArguments { get; set; }
        public Status? Status { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }

    public class GetPowerFlowRealtimeDataResponse : SenecEntities.WebResponse
    {
        public GetPowerFlowRealtimeDataResponse() : base(default, default)
        {
        }

        public Body? Body { get; set; }
        public Head? Head { get; set; }
    }


}
