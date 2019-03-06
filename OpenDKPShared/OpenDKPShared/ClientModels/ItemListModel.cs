using System;

namespace OpenDKPShared.ClientModels
{
    public class ItemListModel
    {
        public string ItemName { get; set; }
        public int ItemID { get; set; }
        public string CharacterName { get; set; }
        public double DKP { get; set; }
        public int IdRaid {get; set;}

        public DateTime Timestamp { get; set; }
        public string Raid { get; set; }
    }
}
