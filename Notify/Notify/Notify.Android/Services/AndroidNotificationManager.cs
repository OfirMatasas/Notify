using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Xamarin.Forms;
using Notify;
using Notify.Droid;
using AndroidApp = Android.App.Application;
using Notify.Services.Location;
using Formula1.Droid;

[assembly: Dependency(typeof(Notify.Droid.Services.AndroidNotificationManager))]
namespace Notify.Droid.Services
{
	public class AndroidNotificationManager : INotificationManager
    {
        private const string channelId = "default";
        private const string channelName = "Default";
        private const string channelDescription = "The default channel for notifications.";
        public const string titleKey = "title";
        public const string messageKey = "message";
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

        public void SendNotification(string title, string message, DateTime? notifyTime = null)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (!notifyTime.Equals(null))
            {
                Intent intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
                PendingIntentFlags pendingIntentFlags = PendingIntentFlags.CancelCurrent | PendingIntentFlags.Mutable | PendingIntentFlags.Immutable;
                PendingIntent pendingIntent;
                AlarmManager alarmManager;
                long triggerTime;

                intent.PutExtra(titleKey, title);
                intent.PutExtra(messageKey, message);

                pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, pendingIntentId++, intent, pendingIntentFlags);
                triggerTime = GetNotifyTime(notifyTime.Value);
                alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
            }
            else
            {
                Show(title, message);
            }
        }

        public void ReceiveNotification(string title, string message)
        {
            NotificationEventArgs args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };

            NotificationReceived?.Invoke(null, args);
        }

        public void Show(string title, string message)
        {
            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable | PendingIntentFlags.Immutable;
            PendingIntent pendingIntent;
            Notification notification;

            intent.PutExtra(titleKey, title);
            intent.PutExtra(messageKey, message);

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
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.notification_icon_background))
                .SetSmallIcon(Resource.Drawable.notification_icon_background)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            return builder.Build();
        }

        void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Java.Lang.String channelNameJava = new Java.Lang.String(channelName);
                NotificationChannel channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
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
    }
}
