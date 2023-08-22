namespace Notify.Core
{
    public class Permission
    {
        public string FriendUsername { get; set; }
        public string LocationNotificationPermission { get; set; }
        public string TimeNotificationPermission { get; set; }
        public string DynamicNotificationPermission { get; set; }
        
        public Permission(string friendUsername, string locationNotificationPermission, string timeNotificationPermission, string dynamicNotificationPermission)
        {
            FriendUsername = friendUsername;
            LocationNotificationPermission = locationNotificationPermission;
            TimeNotificationPermission = timeNotificationPermission;
            DynamicNotificationPermission = dynamicNotificationPermission;
        }
    }
}
