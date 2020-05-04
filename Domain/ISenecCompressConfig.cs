using Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Domain
{
    public interface ISenecCompressConfig
    {
        int MinutesPerSummary { get; }
    }

    public class SenecCompressConfig : ISenecCompressConfig
    {
        public int MinutesPerSummary { get; set; }
    }
}
