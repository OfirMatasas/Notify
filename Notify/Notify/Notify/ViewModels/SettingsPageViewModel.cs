using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class SettingsPageViewModel
    {
        public Command BackCommand { get; set; }
        public Command GoLocationSettingsPageCommand { get; set; }
        public Command GoNotificationSettingsPageCommand { get; set; }
        public Command GoWifiSettingsPageCommand { get; set; }
        public Command GoBluetoothSettingsPageCommand { get; set; }
        public Command DarkModeToggleCommand { get; set; }
        
        public Task Init { get; }
        public bool IsDarkMode { get; set; }

        public SettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
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

        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///home");
        }
        
        private async void onLocationSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync("///location_settings");
        }
        
        private async void onNotificationSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync("///notification_settings");
        }
        
        private async void onWifiSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync("///wifi_settings");
        }
        
        private async void onBluetoothSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync("///bluetooth_settings");
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