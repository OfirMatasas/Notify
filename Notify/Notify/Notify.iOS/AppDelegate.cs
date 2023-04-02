using Foundation;
using Notify.iOS.Notifications;
using UIKit;
using UserNotifications;

namespace Notify.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
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
            return base.FinishedLaunching(app, options);
        }
    }
}
