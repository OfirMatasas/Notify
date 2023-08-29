using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Notify.Droid.Notifications;
using Notify.Helpers;
using Notify.Notifications;
using Notify.Services;
using Notify.ViewModels;
using Notify.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using Environment = System.Environment;

namespace Notify.Droid
{
    [Activity(Label = "Notify", Icon = "@mipmap/icon", Theme = "@style/MainTheme", LaunchMode = LaunchMode.SingleTop,
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private readonly LoggerService r_Logger = AndroidLogger.Instance;
        private Intent serviceIntent;
        private const int RequestCode = 5469;
        internal static readonly string CHANNEL_ID = "my_notification_channel";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Plugin.MaterialDesignControls.Android.Renderer.Init();

            serviceIntent = new Intent(this, typeof(AndroidLocationService));
            SetServiceMethods();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && !Android.Provider.Settings.CanDrawOverlays(this))
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission);
                intent.SetFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }

            App app = new App();

            if (Preferences.ContainsKey(Constants.PREFERENCES_USERNAME) &&
                Preferences.ContainsKey(Constants.PREFERENCES_PASSWORD))
            {
                r_Logger.LogInformation(
                    $"Logging in with credentials from preferences.{Environment.NewLine}UserName: {Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty)}" +
                    $", Password: {Preferences.Get(Constants.PREFERENCES_PASSWORD, string.Empty)}");
                Shell.Current.GoToAsync("//home");
                app.MainPage = Shell.Current;
            }
            else
            {
                app.MainPage = new AppShell();
            }

            LoadApplication(app);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void SetServiceMethods()
        {
            MessagingCenter.Subscribe<StartServiceMessage>(this, "ServiceStarted", message =>
            {
                if (!IsServiceRunning(typeof(AndroidLocationService)))
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        StartForegroundService(serviceIntent);
                    }
                    else
                    {
                        StartService(serviceIntent);
                    }
                }
            });

            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message =>
            {
                if (IsServiceRunning(typeof(AndroidLocationService)))
                    StopService(serviceIntent);
            });
        }

        private bool IsServiceRunning(Type cls)
        {
            ActivityManager manager = (ActivityManager)GetSystemService(ActivityService);

            foreach (ActivityManager.RunningServiceInfo service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.Service.ClassName.Equals(Java.Lang.Class.FromType(cls).CanonicalName))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCode)
            {
                if (Android.Provider.Settings.CanDrawOverlays(this))
                {

                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            createNotificationFromIntent(intent);
        }

        private void createNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.titleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.messageKey);
                string data = intent.GetStringExtra(AndroidNotificationManager.dataKey);
                Core.Notification notification = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Notification>(data);

                NotificationsPageViewModel viewModel = NotificationsPageViewModel.Instance;
                viewModel.ExpandedNotificationId = notification.ID;

                DependencyService.Get<INotificationManager>().ReceiveNotification(title, message, notification);
                App.Current.MainPage.Navigation.PushAsync(new NotificationsPage(viewModel));
            }
        }
    }
}
