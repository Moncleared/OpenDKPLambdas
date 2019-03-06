using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class TicksXCharacters
    {
        public string ClientId { get; set; }
        public int Id { get; set; }
        public int IdCharacter { get; set; }
        public int IdTick { get; set; }

        public Clients Client { get; set; }
        public Characters IdCharacterNavigation { get; set; }
        public Ticks IdTickNavigation { get; set; }
    }
}
