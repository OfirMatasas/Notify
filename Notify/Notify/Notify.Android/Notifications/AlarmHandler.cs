using Android.Content;
using Newtonsoft.Json;
using Notify.Core;
using Notify.Views.Views;

namespace Notify.Droid.Notifications
{
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class AlarmHandler: BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.titleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.messageKey);
                string data = intent.GetStringExtra(AndroidNotificationManager.dataKey);
                Notification notification = JsonConvert.DeserializeObject<Notification>(data);
                AndroidNotificationManager manager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();

                manager.Show(title, message, notification);

                App.Current.MainPage.Navigation.PushAsync(new NotificationDetailsPage(notification));
            }
        }
    }
}