using System;
using Notify.Core;

namespace Notify.Notifications
{
    public interface INotificationManager
    {
        event EventHandler NotificationReceived;
        void Initialize();
        void SendNotification(Notification notification);
        void SendNewsfeed(Newsfeed newsfeed);
        void SendNotification(string title, string message, Notification notification, DateTime? notifyTime = null);
        void ReceiveNotification(string title, string message, Notification notification);
    }
}
