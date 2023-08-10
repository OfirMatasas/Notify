using System;
using Notify.Core;
using Notify.Notifications;
using UserNotifications;
using Xamarin.Forms;

namespace Notify.iOS.Notifications
{
    public class iOSNotificationReceiver : UNUserNotificationCenterDelegate
    {
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            ProcessNotification(notification);
            completionHandler(UNNotificationPresentationOptions.Alert);
        }

        void ProcessNotification(UNNotification notification)
        {
            string title = notification.Request.Content.Title;
            string message = notification.Request.Content.Body;
            string data = notification.Request.Content.UserInfo["data"].ToString();
            Notification notificationData = Newtonsoft.Json.JsonConvert.DeserializeObject<Notification>(data);

            DependencyService.Get<INotificationManager>().ReceiveNotification(title, message, notificationData);
        }    
    }
}
