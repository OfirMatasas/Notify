using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Notify.Services.Location;
using Plugin.FirebasePushNotification;

namespace Notify.Droid
{
    [Application]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";

                //Change for your default notification channel name here
                FirebasePushNotificationManager.DefaultNotificationChannelName = "General";

                FirebasePushNotificationManager.DefaultNotificationChannelImportance = NotificationImportance.Max;
            }

            Firebase.FirebaseApp.InitializeApp(this);

            //If debug you should reset the token each time.
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, true);
#else
            FirebasePushNotificationManager.Initialize(this, false);
#endif

            //Handle notification when app is closed here
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                INotificationManager notificationManager = Xamarin.Forms.DependencyService.Get<INotificationManager>();
                string title = p.Data["title"].ToString();
                string message = p.Data["body"].ToString();

                notificationManager.SendNotification(title, message);
            };
        }
    }
}