using Entities;
using MediatR;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Domain
{
    public class VoltageSummaryFileStore : INotificationHandler<VoltageSummary>
    {
        public Task Handle(VoltageSummary notification, CancellationToken cancellationToken)
        {
            var time = notification.IntervalEndExcluded;
            string path = $@"c:\tmp\volts\voltsummary-{time:yyyy-MM-dd-hh-mm-ss}.json";
            try {File.WriteAllText(path, JsonConvert.SerializeObject(notification)); } catch (Exception) { }
            return Task.CompletedTask;
        }
    }
}
