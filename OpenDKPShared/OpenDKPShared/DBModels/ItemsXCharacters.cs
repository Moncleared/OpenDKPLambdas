using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class ItemsXCharacters
    {
        public int TransactionId { get; set; }
        public string ClientId { get; set; }
        public int CharacterId { get; set; }
        public int ItemId { get; set; }
        public int RaidId { get; set; }
        public double Dkp { get; set; }

        public Characters Character { get; set; }
        public Clients Client { get; set; }
        public Items Item { get; set; }
        public Raids Raid { get; set; }
    }
}
