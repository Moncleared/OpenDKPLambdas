using System;
using System.Collections.Generic;

namespace OpenDKPShared.DBModels
{
    public partial class PendingRequests
    {
        public int Id { get; set; }
        public string Requestor { get; set; }
        public string RequestType { get; set; }
        public string RequestStatus { get; set; }
        public string RequestDetails { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string RequestApprover { get; set; }
        public DateTime ReviewedTimestamp { get; set; }
    }
}
