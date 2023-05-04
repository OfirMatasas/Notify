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

        public static readonly string USERNAME = "lin";
        public static readonly string PASSWORD = "123";

        #region Http Client

        public static readonly string AZURE_FUNCTIONS_APP_BASE_URL = "https://notifymta.azurewebsites.net/api/";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DISTANCE = "distance";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION = "notification";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_TIME = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION}/time";
        public static readonly string AZURE_FUNCTIONS_PATTERN_NOTIFICATION_LOCATION = $"{AZURE_FUNCTIONS_PATTERN_NOTIFICATION}/location";
        public static readonly string AZURE_FUNCTIONS_PATTERN_DESTINATION_UPDATE = "destination/update";
        public static readonly string AZURE_FUNCTIONS_PATTERN_LOGIN = "login";

        #endregion

        #region Vault

        public static readonly string AZURE_KEY_VAULT = "https://notify-keys-vault.vault.azure.net/";
        public static readonly string AZURE_FUNCTIONS_PATTERN_SEND_SMS = "SendSMS";
        
        #endregion
        
        #region Values

        public static readonly int DESTINATION_MAXMIMUM_DISTANCE = 50;
        public static readonly int DISTANCE_UPDATE_THRESHOLD = 30;
        public static readonly int METERS_IN_KM = 1000;
        public static readonly int VERIFICATION_CODE_MAX_LENGTH = 6;

        
        #endregion

        #region Colors

        public static readonly Color VALID_COLOR = Color.SeaGreen;
        public static readonly Color INVALID_COLOR = Color.Red;

        #endregion

        #region Services

        public static readonly string START_LOCATION_SERVICE = "LocationServiceRunning";

        #endregion
    }
}
