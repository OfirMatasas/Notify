using Android.Content;

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
                AndroidNotificationManager manager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();

                manager.Show(title, message);
            }
        }
    }
}