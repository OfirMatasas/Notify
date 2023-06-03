using System.Threading.Tasks;
using Notify.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class SettingsPageViewModel
    {
        public Command GoLocationSettingsPageCommand { get; set; }
        public Command GoNotificationSettingsPageCommand { get; set; }
        public Command GoWifiSettingsPageCommand { get; set; }
        public Command GoBluetoothSettingsPageCommand { get; set; }
        public Command DarkModeToggleCommand { get; set; }
        
        public Task Init { get; }
        public bool IsDarkMode { get; set; }

        public SettingsPageViewModel()
        {
            GoLocationSettingsPageCommand = new Command(onLocationSettingsButtonClicked);
            GoNotificationSettingsPageCommand = new Command(onNotificationSettingsButtonClicked);
            GoWifiSettingsPageCommand = new Command(onWifiSettingsButtonClicked);
            GoBluetoothSettingsPageCommand = new Command(onBluetoothSettingsButtonClicked);
            DarkModeToggleCommand = new Command(DarkModeToggleCommandHandler);

            Init = Initialize();
        }

        public Task Initialize()
        {
            IsDarkMode = Application.Current.UserAppTheme.Equals(OSAppTheme.Dark);
            return Task.CompletedTask;
        }

        private async void onLocationSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_LOCATION_SETTINGS);
        }
        
        private async void onNotificationSettingsButtonClicked()
        {
            AppShell obj = new AppShell();
            obj.MyFunction();
            
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_NOTIFICATIONS_SETTINGS);
        }
        
        private async void onWifiSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_WIFI_SETTINGS);
        }
        
        private async void onBluetoothSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_BLUETOOTH_SETTINGS);
        }
        
        private void DarkModeToggleCommandHandler()
        {
            if (IsDarkMode)
            {
                Application.Current.UserAppTheme = OSAppTheme.Dark;
                Preferences.Set("theme", "dark");
            }
            else
            {
                Application.Current.UserAppTheme = OSAppTheme.Light;
                Preferences.Set("theme", "light");
            }
        }
    }
}
