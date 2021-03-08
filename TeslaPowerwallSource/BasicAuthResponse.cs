using System;
using System.Collections.Generic;
using TeslaEntities;

namespace TeslaPowerwallSource
{
    public class BasicAuthResponse : WebResponse
    {
        public BasicAuthResponse() : base(0, 0) { }

        public string email { get; set; } = "tesla@example.com";
        public string firstname { get; set; } = "Not Tesla";
        public string lastname { get; set; } = "Not Energy";
        public IEnumerable<string> roles { get; set; } = new[] { "Not Home_Owner" };
        public string token { get; set; } = "not set";
        public string provider { get; set; } = "Not basic";
        public string loginTime { get; set; } = DateTimeOffset.Now.ToString("o");
        // {"email":"tesla@example.com","firstname":"Tesla","lastname":"Energy","roles":["Home_Owner"],"token":"","provider":"Basic","loginTime":"2021-03-08T13:41:11.817473081+08:00"}
    }
}