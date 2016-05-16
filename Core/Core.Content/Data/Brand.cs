using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Content.Data
{
    public class Brand
    {
        public Brand()
        {
            Languages = new List<Language>();
            Players = new List<Player>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<Language> Languages { get; set; }
        public ICollection<Player> Players { get; set; }
    }
}