using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Notify.HttpClient;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private string m_Destination;
        private string m_Longitude;
        private string m_Latitude;
        
        public Command backCommand { get; set; }
        public Command UpdateLocationCommand { get; set; }
        public Command DarkModeToggleCommand { get; set; }
        
        public Task Init { get; }
        public bool IsDarkMode { get; set; }

        public SettingsPageViewModel()
        {
            backCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateHomeLocationButtonClicked);
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
            // await Shell.Current.Navigation.PopAsync();
            await Shell.Current.GoToAsync("///profile");
        }

        public string Destination
        {
            get => m_Destination;
            set => SetProperty(ref m_Destination, value);
        }
        public string Longitude
        {
            get => m_Longitude;
            set => SetProperty(ref m_Longitude, value);
        }

        public string Latitude
        {
            get => m_Latitude;
            set => SetProperty(ref m_Latitude, value);
        }
        private async void onUpdateHomeLocationButtonClicked()
        {
            double longitude, latitude;

            Debug.WriteLine($"Location picked: {m_Destination}.{Environment.NewLine}");

            if (double.TryParse(m_Longitude, out longitude) && double.TryParse(m_Latitude, out latitude))
            {
                if (latitude >= -90.0 && latitude <= 90.0 && longitude >= -180.0 && longitude <= 180.0)
                {
                    Debug.WriteLine($"Both Longitude and Latitude are in the right range - updating location.{Environment.NewLine}"); 
                    AzureHttpClient.Instance.updateDestination(m_Destination, new Core.Location(latitude, longitude));
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Longitude and Latitude are not in the right range!", "OK");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Longitude and Latitude must be numbers!", "OK");
            }
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