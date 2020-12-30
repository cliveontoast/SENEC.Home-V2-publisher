using MediatR;
using System;

namespace Entities
{
    public class Publisher : INotification
    {
        public string Name { get; set; }
        public DateTimeOffset LastActive { get; set; }

        public Publisher(string name, DateTimeOffset lastActive)
        {
            Name = name;
            LastActive = lastActive;
        }
    }
}
