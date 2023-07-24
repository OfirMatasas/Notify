using System;
using Notify.Notifications;
using Foundation;
using Notify.Core;
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
                    .SendNotification(notification.Name, notification.Description);
                notification.Status = Constants.NOTIFICATION_STATUS_EXPIRED;
            }
            catch (Exception)
            {
                notification.Status = fallbackStatus;
            }
        }

        public void SendNotification(string title, string message, DateTime? notifyTime = null)
        {
            UILocalNotification notification = new UILocalNotification();
            notification.FireDate = NSDate.FromTimeIntervalSinceNow(0);
            notification.AlertAction = title;
            notification.AlertBody = message;
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        public void ReceiveNotification(string title, string message)
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
