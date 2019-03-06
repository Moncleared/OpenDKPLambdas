using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Pools
    {
        public Pools()
        {
            Raids = new HashSet<Raids>();
        }

        public int IdPool { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }

        public ICollection<Raids> Raids { get; set; }
    }
}
