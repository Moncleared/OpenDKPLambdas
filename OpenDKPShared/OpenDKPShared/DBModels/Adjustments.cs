using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Adjustments
    {
        public string ClientId { get; set; }
        public int IdAdjustment { get; set; }
        public int IdCharacter { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }

        public Clients Client { get; set; }
        public Characters IdCharacterNavigation { get; set; }
    }
}
