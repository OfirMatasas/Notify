using System.Diagnostics;
using Foundation;
using Notify.iOS.Notifications;
using UIKit;
using UserNotifications;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = Notify.Core.Location;

namespace Notify.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private iOSNotificationManager m_NotificationManager = new iOSNotificationManager();
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            UIUserNotificationType notificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge |
                                                       UIUserNotificationType.Sound;
            UIUserNotificationSettings notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(notificationTypes, null);

            global::Xamarin.Forms.Forms.Init();
            Plugin.MaterialDesignControls.iOS.Renderer.Init();
            UNUserNotificationCenter.Current.Delegate = new iOSNotificationReceiver();
            
            app.RegisterUserNotificationSettings(notificationSettings);

            LoadApplication(new App());

            /*UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                 r_logger.LogDebug("Started iOS background task");
                
                var position = Geolocation.GetLocationAsync().Result;
                //
                Location location = new Location(longitude: position.Longitude, latitude: position.Latitude);
            
                MessagingCenter.Send(location, "Location");
                m_NotificationManager.SendNotification("Location changed", "From Background task");
                
                 r_logger.LogDebug("Finished iOS background task");
            });*/
            
            return base.FinishedLaunching(app, options);
        }
    }
}
