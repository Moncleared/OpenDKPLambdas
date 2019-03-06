namespace OpenDKPShared.DBModels
{
    /// <summary>
    /// Another static helper class, mainly  for constants etc
    /// </summary>
    public partial class UserRequests
    {
        public static int TYPE_CHARACTER_ASSIGN = 1;
        public static int TYPE_CHARACTER_UPDATE = 2;
        public static int TYPE_RAIDTICK = 3;

        public static int STATUS_PENDING = 0;
        public static int STATUS_DENIED = 1;
        public static int STATUS_APPROVED = 2;
    }
}
