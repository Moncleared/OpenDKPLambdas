using OpenDKPShared.ClientModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenDKPShared.DBModels
{
    /// <summary>
    /// Class to hold some additional fields we don't pull from the database
    /// </summary>
    public partial class Characters
    {
        [NotMapped]
        public PlayerSummaryModel SummaryModel { get; set; }
    }
}
