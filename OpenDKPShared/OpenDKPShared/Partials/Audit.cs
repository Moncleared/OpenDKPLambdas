namespace OpenDKPShared.DBModels
{
    /// <summary>
    /// Just some static references for readability relating to the Audit Types and Actions etc
    /// </summary>
    public partial class Audit
    {
        public static string ACTION_CHAR_INSERT = "Character Created";
        public static string ACTION_CHAR_UPDATE = "Character Updated";
        public static string ACTION_CHAR_DELETE = "Character Deleted";

        public static string ACTION_RAID_INSERT = "Raid Created";
        public static string ACTION_RAID_UPDATE = "Raid Updated";
        public static string ACTION_RAID_DELETE = "Raid Deleted";

        public static string ACTION_ADJUST_INSERT = "Adjustment Created";
        public static string ACTION_ADJUST_UPDATE = "Adjustment Updated";
        public static string ACTION_ADJUST_DELETE = "Adjustment Deleted";
    }
}
