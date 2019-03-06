using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class AdminSettings
    {
        public string ClientId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTimestamp { get; set; }

        public Clients Client { get; set; }
    }
}
