using Entities;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.WebApp.Dto
{
    public class PublishersDto
    {
        public PublishersDto(IEnumerable<PublisherReadModel> result)
        {
            Publishers = result.Select(a => new PublisherDto(a.Name, a.LastActive));
        }

        public IEnumerable<PublisherDto> Publishers { get; }

        public class PublisherDto
        {
            public string Name { get; set; }
            public long LastActive { get; set; }

            public PublisherDto(string name, DateTimeOffset lastActive)
            {
                Name = name;
                LastActive = lastActive.ToUnixTimeMilliseconds();
            }
        }
    }
}
