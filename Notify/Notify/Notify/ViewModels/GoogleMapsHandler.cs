using System;
using Android.Content;
using Android.Content.PM;
using Notify.Services;

namespace Notify.ViewModels
{
    public class GoogleMapsHandler
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private static GoogleMapsHandler s_Instance;
        private readonly Context r_Context;

        private GoogleMapsHandler(Context context)
        {
            r_Context = context;
        }

        public static void Initialize(Context context)
        {
            if (s_Instance == null)
            {
                s_Instance = new GoogleMapsHandler(context);
            }
        }

        public static GoogleMapsHandler GetInstance()
        {
            if (s_Instance == null)
            {
                throw new InvalidOperationException("MyMapHandler has not been initialized. Call Initialize first.");
            }
            
            return s_Instance;
        }

        public void OpenGoogleMapsNavigation(double latitude, double longitude)
        {
            //if (IsGoogleMapsInstalled())
            if(true)
            {
                var uri = Android.Net.Uri.Parse($"google.navigation:q={latitude},{longitude}");
                var intent = new Intent(Intent.ActionView, uri);
                intent.SetPackage("com.google.android.apps.maps");
                r_Context.StartActivity(intent);
            }
            else
            {
                r_Logger.LogWarning("Google Maps is not installed on the device.");
            }
        }

        private bool IsGoogleMapsInstalled()
        {
            bool res;
            
            try
            {
                 var uri = Android.Net.Uri.Parse("com.google.android.apps.maps");
                 var intent = new Intent(Intent.ActionView, uri);
                 intent.SetPackage("com.google.android.apps.maps");
                 res = r_Context.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly).Count > 0;
            }
            catch (Exception)
            {
                r_Logger.LogWarning("Something went wrong with detecting the Google Maps application on the device.");
                res = false;
            }

            return res;
        }
    }
}