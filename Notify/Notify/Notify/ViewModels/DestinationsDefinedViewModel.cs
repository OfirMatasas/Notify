using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public sealed class DestinationsDefinedViewModel :INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private ObservableCollection<Destination> m_ScrollViewContent;
        
        public Command LocationButtonCommand { get; set; }
        public Command BlueToothButtonCommand { get; set; }
        public Command WifiButtonCommand { get; set; }
        
        private List<Destination> Destinations { get; set; }
        
        #region Constructor
        
        public DestinationsDefinedViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            
            LocationButtonCommand = new Command(onLocationButtonPressed);
            BlueToothButtonCommand = new Command(onBlueToothButtonPressed);
            WifiButtonCommand = new Command(onWifiButtonPressed);
            
            ScrollViewContent = new ObservableCollection<Destination>();
            
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, String.Empty);
            Destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
        }

        #endregion
  
        public ObservableCollection<Destination> ScrollViewContent
        {
            get => m_ScrollViewContent;
            set
            {
                m_ScrollViewContent = value;
                OnPropertyChanged(nameof(ScrollViewContent));
            }
        }       
        
        private void onLocationButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (destination.IsDynamic)
                    continue;
                
                ScrollViewContent.Add(new Destination(destination.Name)
                {
                    LastUpdatedLocation = destination.LastUpdatedLocation,
                    Address = destination.Address
                });
            }

            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onBlueToothButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (destination.IsDynamic)
                    continue;
                
                // if (!string.IsNullOrWhiteSpace(destination.Bluetooth))
                // {
                    ScrollViewContent.Add(new Destination(destination.Name)
                    {
                        Bluetooth = destination.Bluetooth
                    });
                // }
            }
            
            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onWifiButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (destination.IsDynamic)
                    continue;
                
                // if (!string.IsNullOrWhiteSpace(destination.SSID))
                // {
                    ScrollViewContent.Add(new Destination(destination.Name)
                    {
                        SSID = destination.SSID
                    });
                // }
            }
            
            OnPropertyChanged(nameof(ScrollViewContent));
        }
        
        #region Back_Button

        public Command BackCommand { get; set; }

        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_SETTINGS);
        }

        #endregion
        
        #region Property_Changed

        public event PropertyChangedEventHandler PropertyChanged;

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
        
        #endregion
    }
}