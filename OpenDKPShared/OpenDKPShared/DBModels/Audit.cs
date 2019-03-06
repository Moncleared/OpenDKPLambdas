using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class Audit
    {
        public string ClientId { get; set; }
        public int Id { get; set; }
        public string CognitoUser { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public Clients Client { get; set; }
    }
}
