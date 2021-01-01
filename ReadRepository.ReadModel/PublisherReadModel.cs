using System;

namespace Repository.Model
{
    public class PublisherReadModel
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public DateTimeOffset LastActive { get; set; }

        public PublisherReadModel(string name, string key, DateTimeOffset lastActive)
        {
            Name = name;
            Key = key;
            LastActive = lastActive;
        }
    }
}
