using Plugin.FirebasePushNotification;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: ExportFont("FontAwesome-Regular.ttf", Alias = "FontAwesome_Regular")]
[assembly: ExportFont("FontAwesome-Solid.ttf", Alias = "FontAwesome_Solid")]

[assembly: ExportFont("Exo-Black.ttf", Alias = "Exo_Black")]
[assembly: ExportFont("Exo-Bold.ttf", Alias = "Exo_Bold")]
[assembly: ExportFont("Exo-Medium.ttf", Alias = "Exo_Medium")]
[assembly: ExportFont("Exo-Regular.ttf", Alias = "Exo_Regular")]

namespace Notify
{
    public partial class App : Application
    {
        public App() 
        {
            InitializeComponent();
            SetAppTheme();
            MainPage = new AppShell();
            CrossFirebasePushNotification.Current.Subscribe("all");
            CrossFirebasePushNotification.Current.OnTokenRefresh += Current_OnTokenRefresh;
        }

        private void Current_OnTokenRefresh(object source, FirebasePushNotificationTokenEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Token: {e.Token}");
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            // Handle when your app starts
        }

        protected override void OnResume()
        {
            // Handle when your app sleeps
        }

        private void SetAppTheme()
        {
            var theme = Preferences.Get("theme", string.Empty);
            if (string.IsNullOrEmpty(theme) || theme == "light")
            {
                Application.Current.UserAppTheme = OSAppTheme.Light;
            }
            else
            {
                Application.Current.UserAppTheme = OSAppTheme.Dark;
            }
        }
    }
}