using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Raids
    {
        public Raids()
        {
            ItemsXCharacters = new HashSet<ItemsXCharacters>();
            Ticks = new HashSet<Ticks>();
        }

        public string ClientId { get; set; }
        public int IdRaid { get; set; }
        public int IdPool { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTimestamp { get; set; }

        public Clients Client { get; set; }
        public Pools IdPoolNavigation { get; set; }
        public ICollection<ItemsXCharacters> ItemsXCharacters { get; set; }
        public ICollection<Ticks> Ticks { get; set; }
    }
}
