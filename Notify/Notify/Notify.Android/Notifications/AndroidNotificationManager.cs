using System;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Newtonsoft.Json;
using Notify.Core;
using Notify.Helpers;
using Notify.Notifications;
using Notify.Services;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;
using Notification = Android.App.Notification;

[assembly: Dependency(typeof(Notify.Droid.Notifications.AndroidNotificationManager))]
namespace Notify.Droid.Notifications
{
    public class AndroidNotificationManager : INotificationManager
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private const string channelId = "default";
        private const string channelName = "Default";
        private const string channelDescription = "The default channel for notifications.";
        public const string titleKey = "title";
        public const string messageKey = "message";
        public const string dataKey = "data";
        private bool channelInitialized = false;
        private int messageId = 0;
        private int pendingIntentId = 0;
        private NotificationManager manager;
        public event EventHandler NotificationReceived;
        public static AndroidNotificationManager Instance { get; private set; }
        public AndroidNotificationManager() => Initialize();

        public void Initialize()
        {
            if (Instance == null)
            {
                CreateNotificationChannel();
                Instance = this;
            }
        }
        
        public void SendNotification(Core.Notification notification)
        {
            string fallbackStatus = notification.Status;
            
            notification.Status = Constants.NOTIFICATION_STATUS_EXPIRED;
            r_Logger.LogDebug(
                $"Sending notification with name: {notification.Name} and description: {notification.Description}");
            try
            {
                DependencyService.Get<INotificationManager>()
                    .SendNotification(notification.Name, notification.Description, notification);
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error sending notification: {ex.Message}");
                notification.Status = fallbackStatus;
            }
        }

        public void SendNotification(string title, string message, Core.Notification notification, DateTime? notifyTime = null)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (!notifyTime.Equals(null))
            {
                Intent intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
                PendingIntentFlags pendingIntentFlags = PendingIntentFlags.CancelCurrent | PendingIntentFlags.Mutable |
                                                        PendingIntentFlags.Immutable;
                PendingIntent pendingIntent;
                AlarmManager alarmManager;
                long triggerTime;

                intent.PutExtra(titleKey, title);
                intent.PutExtra(messageKey, message);
                intent.PutExtra("data", JsonConvert.SerializeObject(notification));

                pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, pendingIntentId++, intent, pendingIntentFlags);
                triggerTime = GetNotifyTime(notifyTime.Value);
                alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
            }
            else
            {
                Show(title, message, notification);
            }
        }

        public void ReceiveNotification(string title, string message, Core.Notification notification)
        {
            NotificationEventArgs args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };

            NotificationReceived?.Invoke(null, args);
        }

        public void Show(string title, string message, Core.Notification data)
        {
            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable |
                                                    PendingIntentFlags.Immutable;
            PendingIntent pendingIntent;
            Notification notification;

            intent.PutExtra(titleKey, title);
            intent.PutExtra(messageKey, message);
            intent.PutExtra("data", JsonConvert.SerializeObject(data));

            pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId++, intent, pendingIntentFlags);
            notification = buildNotification(title, message, pendingIntent);

            manager.Notify(messageId++, notification);
        }

        private static Notification buildNotification(string title, string message, PendingIntent pendingIntent)
        {
            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetSmallIcon(Resource.Mipmap.icon_round)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            return builder.Build();
        }

        void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Java.Lang.String channelNameJava = new Java.Lang.String(channelName);
                NotificationChannel channel =
                    new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                    {
                        Description = channelDescription
                    };

                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }

        long GetNotifyTime(DateTime notifyTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;

            return utcAlarmTime; // milliseconds
        }
        
        public void SendNewsfeed(Newsfeed newsfeed)
        {
            r_Logger.LogDebug($"Sending newsfeed with title: {newsfeed.Title} and content: {newsfeed.Content}");
            
            try
            {
                DependencyService.Get<INotificationManager>()
                    .SendNotification(newsfeed.Title, newsfeed.Content, null);
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error sending notification: {ex.Message}");
            }
        }
    }
}
