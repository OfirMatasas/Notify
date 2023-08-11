using System;
using Notify.Notifications;
using Foundation;
using Notify.Core;
using Notify.Helpers;
using UIKit;
using Notify.Helpers;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(Notify.iOS.Notifications.iOSNotificationManager))]
namespace Notify.iOS.Notifications
{
    public class iOSNotificationManager : INotificationManager
    {
        private bool hasNotificationsPermission;
        
        public event EventHandler NotificationReceived;

        public void Initialize()
        {
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
            {
                hasNotificationsPermission = approved;
            });
        }
        
        public void SendNotification(Notification notification)
        {
            string fallbackStatus = notification.Status;
            
            notification.Status = Constants.NOTIFICATION_STATUS_SENDING;

            try
            {
                DependencyService.Get<INotificationManager>()
                    .SendNotification(notification.Name, notification.Description, notification);
                notification.Status = Constants.NOTIFICATION_STATUS_EXPIRED;
            }
            catch (Exception)
            {
                notification.Status = fallbackStatus;
            }
        }

        public void SendNotification(string title, string message, Notification notification, DateTime? notifyTime = null)
        {
            UILocalNotification localNotification = new UILocalNotification();
            localNotification.FireDate = NSDate.FromTimeIntervalSinceNow(0);
            localNotification.AlertAction = title;
            localNotification.AlertBody = message;
            UIApplication.SharedApplication.ScheduleLocalNotification(localNotification);
        }

        public void ReceiveNotification(string title, string message, Notification notification)
        {
            NotificationEventArgs args = new NotificationEventArgs
            {
                Title = title,
                Message = message
            };
            
            NotificationReceived?.Invoke(null, args);
        }
    }
}
