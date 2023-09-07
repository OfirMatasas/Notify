using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Services;
using Xamarin.Essentials;

namespace Notify.Helpers
{
    public static class Utils
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private static readonly object r_Lock = new object();

        public static void CheckForExpiredPendingTimeNotifications(List<Notification> notifications)
        {
            List<Notification> expiredPendingTimeNotifications = notifications.FindAll(notification =>
                notification.Status.Equals(Constants.NOTIFICATION_STATUS_PENDING) &&
                notification.Type.Equals(NotificationType.Time) &&
                (DateTime)notification.TypeInfo < DateTime.Now);

            if (expiredPendingTimeNotifications.Count > 0)
            {
                r_Logger.LogInformation($"Expired pending time notifications: {string.Join(", ", expiredPendingTimeNotifications.Select(notification => notification.Name))}");
            }

            foreach (Notification expiredPendingTimeNotification in expiredPendingTimeNotifications)
            {
                expiredPendingTimeNotification.Status = Constants.NOTIFICATION_STATUS_EXPIRED;
            }
            
            UpdateNotificationsStatus(expiredPendingTimeNotifications, Constants.NOTIFICATION_STATUS_EXPIRED);
        }
        
        public static void UpdateNotificationsStatus(List<Notification> notificationsToUpdate, string newStatus)
        {
            lock (r_Lock)
            {
                string json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
                List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(json);

                notifications.ForEach(notification =>
                {
                    if (notificationsToUpdate.Any(arrivedNotification =>
                            arrivedNotification.ID.Equals(notification.ID)))
                    {
                        notification.Status = newStatus;
                        r_Logger.LogInformation($"Updated status of notification {notification.ID} to '{newStatus}'");
                    }
                });

                Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
                AzureHttpClient.Instance.UpdateNotificationsStatus(notificationsToUpdate, newStatus);
            }
        }
    }
}
