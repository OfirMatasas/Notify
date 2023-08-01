using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Notify.Core;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public sealed class ProfilePageViewModel : INotifyPropertyChanged
    {
        public Command LocationButtonCommand { get; set; }
        public Command BlueToothButtonCommand { get; set; }
        public Command WifiButtonCommand { get; set; }

        private string destinationsJson;
        public List<Destination> Destinations { get; private set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private string m_UserName;
        public string UserName 
        { 
            get => m_UserName;
            set
            {
                m_UserName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        
        public ProfilePageViewModel()
        {
            UserName = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
            
            LocationButtonCommand = new Command(onLocationButtonPressed);
            BlueToothButtonCommand = new Command(onBlueToothButtonPressed);
            WifiButtonCommand = new Command(onWifiButtonPressed);
            
            destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, String.Empty);
            Destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
        }

        private void onWifiButtonPressed()
        {
            App.Current.MainPage.DisplayAlert("Wifi Network", $"{Destinations.First().SSID}", "OK");
        }

        private void onBlueToothButtonPressed()
        {
            App.Current.MainPage.DisplayAlert("Bluetooth Device", $"{Destinations.First().Bluetooth}", "OK");
        }

        private async void onLocationButtonPressed()
        {
            IEnumerable<string> destinationNames;
            string message;

            destinationNames = Destinations.Select(d => d.Name);
            message = string.Join(", ", destinationNames);

            await App.Current.MainPage.DisplayAlert("Destinations", message, "OK");
        }
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
