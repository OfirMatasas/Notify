using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] colorArray;
            if (value is bool booleanValue && parameter is string colors)
            {
                colorArray = colors.Split(';');
                if (colorArray.Length == 2)
                {
                    return booleanValue ? Color.FromHex(colorArray[0]) : Color.FromHex(colorArray[1]);
                }
            }

            return Color.Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class TypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color notificationColor = Color.Default;
            
            if (value is NotificationType type)
            {
                switch (type)
                {
                    case NotificationType.Location:
                        notificationColor = Constants.LOCATION_NOTIFICATION_COLOR;
                        break;
                    case NotificationType.Dynamic:
                        notificationColor = Constants.DYNAMIC_NOTIFICATION_COLOR;
                        break;
                    case NotificationType.Time:
                        notificationColor = Constants.TIME_NOTIFICAATION_COLOR;
                        break;
                }
            }

            return notificationColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationType type)
            {
                switch (type)
                {
                    case NotificationType.Location:
                        return ImageSource.FromFile("location_colored_icon"); 
                    case NotificationType.Dynamic:
                        return ImageSource.FromFile("dynamic_location_colored_icon");
                    case NotificationType.Time:
                        return ImageSource.FromFile("time_colored_icon");
                    default:
                        return null; 
                }
            }
            
            return null;
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
                notificationTypeValue = Convert.ToString(notification.notification.location);
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
            Console.WriteLine("Address: " + (string)(destination.location.address ?? "null"));
            return new Destination((string)destination.location.name)
            {
                Locations = new List<Location>
                {
                    new Location(
                        longitude: (double)(destination.location.longitude ?? 0),
                        latitude: (double)(destination.location.latitude ?? 0),
                        address: (string)(destination.location.address ?? ""))
                },
                SSID = (string)(destination.location.ssid ?? ""),
                Bluetooth = (string)(destination.location.device ?? ""),
                Address = (string)(destination.location.address ?? "")
            };
        }
    }
}
