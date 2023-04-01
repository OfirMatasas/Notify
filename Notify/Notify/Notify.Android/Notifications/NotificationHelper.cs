using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace Notify.Droid
{
    internal class NotificationHelper
    {
        private static string foregroundChannelId = "9001";
        private static Context context = Application.Context;

        public Notification GetServiceStartedNotification()
        {
            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Notify tracking location service")
                .SetContentText("Your location is being tracked")
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                if (notificationManager != null)
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
                .SetContentTitle("Ofir testing")
                .SetContentText("You've arrived your destination!")
                .SetSmallIcon(Resource.Drawable.ic_clock_black_24dp)
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
