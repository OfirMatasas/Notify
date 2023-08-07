using System;
using System.Collections.Generic;
using System.Globalization;
using Notify.Core;
using Xamarin.Forms;

namespace Notify.Helpers
{
    public class NullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string strValue && !string.IsNullOrEmpty(strValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public static class Converter
    {
        public static Notification ToNotification(dynamic notification)
        {
            NotificationType notificationType;
            string notificationTypeAsString, activation = string.Empty;
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

            if (notification.notification.activation != null)
            {
                activation = (string)notification.notification.activation;
            }
            else if (notificationType.Equals(NotificationType.Dynamic))
            {
                activation = Constants.NOTIFICATION_ACTIVATION_ARRIVAL;
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
                activation: activation,
                permanent: notification.notification.permanent != null && (bool)notification.notification.permanent,
                target: (string)notification.user);
        }
        
        public static User ToFriend(dynamic friend)
        {
            return new User(
                name: (string)friend.name, 
                username: (string)friend.userName, 
                telephone: (string)friend.telephone);
        }
        
        public static Destination ToDestination(dynamic destination)
        {
            return new Destination((string)destination.location.name)
            {
                Locations = new List<Location>
                {
                    new Location(
                        longitude: (double)(destination.location.longitude ?? 0),
                        latitude: (double)(destination.location.latitude ?? 0))
                },
                SSID = (string)(destination.location.ssid ?? ""),
                Bluetooth = (string)(destination.location.device ?? "")
            };
        }
    }
}
