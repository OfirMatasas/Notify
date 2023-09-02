using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.Helpers
{
    public static class Constants
    {
        #region BaseURL

        #if DEBUG
                public static string ImageApiBaseUrl = "http://10.0.2.2:4000/";
                public static string InformationsApiBaseUrl = "http://10.0.2.2:5000/";
        #else
                public static string ImageApiBaseUrl = "Github Url";
                public static string InformationsApiBaseUrl = "Github Url";
        #endif

        #endregion

        #region PopupSize

        public static Size PopupSizeSmall => new Size(0.8 * (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density), 0.5 * (DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density));
        
        public static Size PopupSizeMedium => new Size(0.8 * (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density), 0.7 * (DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density));

        public static Size PopupSizeLarge => new Size(0.9 * (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density), 0.8 * (DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density));

        #endregion

        #region Seasons

        public static List<int> GetSeasonsList()
        {
            var seasons = new List<int>();
            for(int i = DateTime.Now.Year; i >= 1950; i--)
            {
                seasons.Add(i);
            }
            return seasons;
        }

        #endregion

        #region Race Types

        public static List<string> GetRaceTypesList()
        {
            return new List<string>() { "Race", "Qualification", "Sprint" };
        }

        #endregion

        #region Team Color

        public static Dictionary<string, string> TeamColors = new Dictionary<string, string>()
        {
            { "alfa", "#B12039"},
            { "alphatauri", "#4E7C9B"},
            { "alpine", "#2293D1"},
            { "aston_martin", "#2D826D"},
            { "ferrari", "#ED1C24"},
            { "haas", "#B6BABD"},
            { "mclaren", "#F58020"},
            { "mercedes", "#6CD3BF"},
            { "red_bull", "#1E5BC6"},
            { "williams", "#37BEDD"},
        };

        #endregion

        #region Location_Options

        public static readonly List<string> LOCATIONS_LIST = new List<string>
        {
            "Home",
            "Work",
            "School"
        };
        
        public static readonly List<string> DYNAMIC_PLACE_LIST = new List<string>
        {
            "ATM",
            "Bank",
            "Pharmacy",
            "Supermarket"
        };

        public static readonly int HALF_KM = 500;
        public static readonly int ONE_KM = 1000;

        #endregion
        
        #region Notification

        public static readonly string TIME = "Time";
        public static readonly string LOCATION = "Location";
        public static readonly string DYNAMIC = "Dynamic";
        
        public static readonly List<string> NOTIFICATION_OPTIONS_LIST = new List<string>
        {
            TIME,
            LOCATION,
            DYNAMIC
        };
        
        public static readonly List<string> ACTIVATION_OPTIONS_LIST = new List<string>
        {
            "Arrival",
            "Leave"
        };
        
        public static readonly string NOTIFICATION_ACTIVATION_ARRIVAL = "Arrival";
        public static readonly string NOTIFICATION_ACTIVATION_LEAVE = "Leave";
        
        public static readonly string NOTIFICATION_STATUS_PENDING = "Pending";
        public static readonly string NOTIFICATION_STATUS_ACTIVE = "Active";
        public static readonly string NOTIFICATION_STATUS_DECLINED = "Declined";
        public static readonly string NOTIFICATION_STATUS_ARRIVED = "Arrived";
        public static readonly string NOTIFICATION_STATUS_EXPIRED = "Expired";
        
        public static readonly string NOTIFICATION_PERMISSION_ALLOW = "Allow";
        public static readonly string NOTIFICATION_PERMISSION_DISALLOW = "Disallow";

        #endregion

        #region Azure Http Client

        public static readonly string AZURE_FUNCTIONS_APP_BASE_URL = "https://notifymta.azurewebsites.net/api/";
        public static readonly string AZURE_FUNCTIONS_PATTERN_USER = "user";
        public static readonly string AZURE_FUNCTIONS_PATTERN_UPDATE_USER = AZURE_FUNCTIONS_PATTERN_USER + "/profilePicture";
        public static readonly string AZURE_FUNCTIONS_PATTERN_USERS_NOT_FRIENDS = AZURE_FUNCTIONS_PATTERN_USER + "/notFriends";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION = "notification";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION}/create";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION_TIME = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION}/time";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION_LOCATION = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION}/location";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION_DYNAMIC = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION}/dynamic";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION}/update";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_TIME = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE}/time";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_LOCATION = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE}/location";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_DYNAMIC = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE}/dynamic";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_STATUS = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION}/status";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_RENEW = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION}/renew";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DESTINATION = "destination";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DESTINATION_UPDATE = AZURE_FUNCTIONS_PATTERN_DESTINATION + "/update";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DESTINATION_SUGGESTIONS = AZURE_FUNCTIONS_PATTERN_DESTINATION + "/suggestions";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DESTINATION_COORDINATES= AZURE_FUNCTIONS_PATTERN_DESTINATION + "/coordinates";
        public static readonly string AZURE_FUNCTIONS_PATTERN_PERMISSION = "permission";
        public static readonly string AZURE_FUNCTIONS_PATTERN_LOGIN = "login";
        public static readonly string AZURE_FUNCTIONS_PATTERN_REGISTER = "register";
        public static readonly string AZURE_FUNCTIONS_PATTERN_FRIEND = "friend";
        public static readonly string AZURE_FUNCTIONS_PATTERN_FRIEND_REQUEST = AZURE_FUNCTIONS_PATTERN_FRIEND + "/request";
        public static readonly string AZURE_FUNCTIONS_PATTERN_CHECK_USER_EXISTS = "checkUserExists";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DYNAMIC_DESTINATION = AZURE_FUNCTIONS_PATTERN_DESTINATION + "/dynamic";
        public static readonly string AZURE_FUNCTIONS_PATTERN_REJECT_FRIEND_REQUEST = AZURE_FUNCTIONS_PATTERN_FRIEND + "/reject";
        public static readonly string AZURE_FUNCTIONS_PATTERN_ACCEPT_FRIEND_REQUEST = AZURE_FUNCTIONS_PATTERN_FRIEND + "/accept";
        public static readonly string AZURE_FUNCTIONS_PATTERN_UPLOAD_PROFILE_PICTURE_TO_BLOB = "uploadProfilePicture";
        public static readonly string AZURE_FUNCTIONS_DEFAULT_USER_PROFILE_PICTURE = "https://notifyblobstorage.blob.core.windows.net/notifycontainer/4199b89b-48e4-477c-9496-50c340ccd882.jpg";

        #endregion

        #region Vault

        public static readonly string AZURE_FUNCTIONS_PATTERN_SEND_SMS = "SendSMS";
        
        #endregion

        #region Shell Navigation
            
        public static readonly string SHELL_NAVIGATION_LOGIN = "///login";
        public static readonly string SHELL_NAVIGATION_MAIN = "///main";
        public static readonly string SHELL_NAVIGATION_REGISTER = "///register";
        public static readonly string SHELL_NAVIGATION_SETTINGS = "///settings";
        public static readonly string SHELL_NAVIGATION_NOTIFICATIONS = "///notifications";
        public static readonly string SHELL_NAVIGATION_CREATE_NOTIFICATION = "///create_notification";
        public static readonly string SHELL_NAVIGATION_LOCATION_SETTINGS = "///location_settings";
        public static readonly string SHELL_NAVIGATION_NOTIFICATIONS_SETTINGS = "///notification_settings";
        public static readonly string SHELL_NAVIGATION_WIFI_SETTINGS = "///wifi_settings";
        public static readonly string SHELL_NAVIGATION_BLUETOOTH_SETTINGS = "///bluetooth_settings";
        
        #endregion
        
        #region Values

        public static readonly int DESTINATION_MAXMIMUM_DISTANCE = 50;
        public static readonly int DYANMIC_DESTINATION_UPDATE_DISTANCE_THRESHOLD = 500;
        public static readonly int DISTANCE_UPDATE_THRESHOLD = 30;
        public static readonly int METERS_IN_KM = 1000;
        public static readonly int VERIFICATION_CODE_MAX_LENGTH = 6;
        public static readonly int LATITUDE_MAX = 90;
        public static readonly int LATITUDE_MIN = -90;
        public static readonly int LONGITUDE_MAX = 180;
        public static readonly int LONGITUDE_MIN = -180;

        #endregion

        #region Prefrences_Keys

        public static readonly string PREFERENCES_NOTIFICATIONS = "Notifications";
        public static readonly string PREFERENCES_DESTINATIONS = "Destinations";
        public static readonly string PREFERENCES_FRIENDS = "Friends";
        public static readonly string PREFERENCES_PENDING_FRIEND_REQUESTS = "PendingFriendRequests";
        public static readonly string PREFERENCES_USERNAME = "NotifyUserName";
        public static readonly string PREFERENCES_PASSWORD = "NotifyPassword";
        public static readonly string PREFERENCES_NOT_FRIENDS_USERS = "NotFriendsUsers";
        public static readonly string PREFERENCES_FRIENDS_PERMISSIONS = "FriendsPermissions";
        public static readonly string PREFERENCES_USER_OBJECT = "UserObject";

        #endregion

        #region Filter_Types
        
        public static readonly string FILTER_TYPE_ACTIVE = "Active";
        public static readonly string FILTER_TYPE_PENDING = "Pending";
        public static readonly string FILTER_TYPE_EXPIRED = "Expired";
        public static readonly string FILTER_TYPE_PERMANENT = "Permanent";
        public static readonly string FILTER_TYPE_LOCATION = "Location";
        public static readonly string FILTER_TYPE_DYNAMIC_LOCATION = "Dynamic Location";
        public static readonly string FILTER_TYPE_TIME = "Time";
        public static readonly string FILTER_TYPE_ALL = "All Notifications";
        
        #endregion

        #region Colors

        public static readonly Color VALID_COLOR = Color.SeaGreen;
        public static readonly Color INVALID_COLOR = Color.Red;
        public static readonly Color LOCATION_NOTIFICATION_COLOR = Color.FromHex("#B96CBD");
        public static readonly Color DYNAMIC_NOTIFICATION_COLOR = Color.FromHex("#49A24D");
        public static readonly Color TIME_NOTIFICAATION_COLOR = Color.FromHex("#FDA838");

        #endregion

        #region Services

        public static readonly string START_LOCATION_SERVICE = "LocationServiceRunning";

        #endregion
    }
}
