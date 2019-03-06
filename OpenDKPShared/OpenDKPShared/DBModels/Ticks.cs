using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Ticks
    {
        public Ticks()
        {
            TicksXCharacters = new HashSet<TicksXCharacters>();
        }

        public string ClientId { get; set; }
        public int TickId { get; set; }
        public int RaidId { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }

        public Clients Client { get; set; }
        public Raids Raid { get; set; }
        public ICollection<TicksXCharacters> TicksXCharacters { get; set; }
    }
}
