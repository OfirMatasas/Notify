using System;
using Notify;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Formula1.Droid;

namespace Notify.Droid.Services
{
	public class NotificationHelper
	{
        private static string foregroundChannelId = "9001";
        private static Context context = global::Android.App.Application.Context;

        public Notification GetServiceStartedNotification()
        {
            Intent intent = new Intent(context, typeof(MainActivity));
            PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable;
            NotificationCompat.Builder notificationBuilder;
            PendingIntent pendingIntent;

            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            pendingIntent = PendingIntent.GetActivity(context, 0, intent, pendingIntentFlags);

            notificationBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Xamarin.Forms Background Tracking Example")
                .SetContentText("Your location is being tracked")
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                NotificationManager notificationManager;

                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

                if (!notificationManager.Equals(null))
                {
                    notificationBuilder.SetChannelId(foregroundChannelId);
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notificationBuilder.Build();
        }

        public Notification GetServiceArrivedToDestinationNotification()
        {
            Intent intent = new Intent(context, typeof(MainActivity));
            PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable | PendingIntentFlags.Immutable;
            NotificationCompat.Builder notificationBuilder;
            PendingIntent pendingIntent;

            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            pendingIntent = PendingIntent.GetActivity(context, 0, intent, pendingIntentFlags);

            notificationBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Xamarin.Forms Background Tracking Example")
                .SetContentText("You've arrived your destination!")
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                NotificationManager notificationManager;

                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

                if (!notificationManager.Equals(null))
                {
                    notificationBuilder.SetChannelId(foregroundChannelId);
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notificationBuilder.Build();
        }
    }
}
