﻿using System.Threading.Tasks;
using Notify.Helpers;
using Notify.Views;
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
            await Shell.Current.Navigation.PushAsync(new LocationSettingsPage());
        }
        
        private async void onNotificationSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new NotificationSettingsPage());
        }
        
        private async void onWifiSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new WifiSettingsPage());
        }
        
        private async void onBluetoothSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new BluetoothSettingsPage());
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
