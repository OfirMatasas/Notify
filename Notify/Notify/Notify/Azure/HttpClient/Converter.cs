using System;
using Notify.Core;
using Notify.Helpers;

namespace Notify.Azure.HttpClient
{
    public static class Converter
    {
        public static Notification ToNotification(dynamic notification)
        {
            NotificationType notificationType;
            string notificationTypeAsString;
            object notificationTypeValue;
            
            if(notification.notification.location != null)
            {
                notificationTypeValue = notification.notification.location;
                notificationTypeAsString = Convert.ToString(notification.notification.type);
                notificationType = notificationTypeAsString.Equals(Constants.DYNAMIC)
                    ? NotificationType.Dynamic
                    : NotificationType.Location;
            }
            else
            {
                notificationTypeValue = DateTimeOffset.FromUnixTimeSeconds((long)notification.notification.timestamp).LocalDateTime;
                notificationType = NotificationType.Time;
            }

            return new Notification(
                id: (string)notification.id,
                name: (string)notification.notification.name,
                description: (string)(notification.description ?? notification.info),
                creationDateTime: DateTimeOffset.FromUnixTimeSeconds((long)notification.creation_timestamp)
                    .LocalDateTime,
                status: (string)notification.status,
                creator: (string)notification.creator,
                type: notificationType,
                typeInfo: notificationTypeValue,
                target: (string)notification.user);
        }
    }
}
