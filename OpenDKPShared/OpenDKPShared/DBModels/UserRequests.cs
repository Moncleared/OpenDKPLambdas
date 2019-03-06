using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class UserRequests
    {
        public string ClientId { get; set; }
        public int Id { get; set; }
        public string Requestor { get; set; }
        public int RequestType { get; set; }
        public int RequestStatus { get; set; }
        public string RequestDetails { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string RequestApprover { get; set; }
        public DateTime ReviewedTimestamp { get; set; }

        public Clients Client { get; set; }
    }
}
