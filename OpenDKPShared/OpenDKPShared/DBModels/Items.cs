using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Items
    {
        public Items()
        {
            ItemsXCharacters = new HashSet<ItemsXCharacters>();
        }

        public int IdItem { get; set; }
        public string Name { get; set; }
        public string Lucylink { get; set; }

        public ICollection<ItemsXCharacters> ItemsXCharacters { get; set; }
    }
}
