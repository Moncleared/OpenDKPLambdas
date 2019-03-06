using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class UserXCharacter
    {
        public string ClientId { get; set; }
        public string User { get; set; }
        public int IdCharacter { get; set; }
        public string ApprovedBy { get; set; }

        public Clients Client { get; set; }
        public Characters IdCharacterNavigation { get; set; }
    }
}
