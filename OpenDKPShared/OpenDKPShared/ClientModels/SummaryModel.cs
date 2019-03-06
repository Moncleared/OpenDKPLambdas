using System;
using System.Collections.Generic;

namespace OpenDKPShared.ClientModels
{
    public class SummaryModel
    {
        public SummaryModel()
        {
            Models = new List<PlayerSummaryModel>();
            AsOfDate = DateTime.Now;
        }
        public List<PlayerSummaryModel> Models { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}
