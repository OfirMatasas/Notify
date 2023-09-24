using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Notify.Core;
using Notify.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public sealed class DefinedDestinationsViewModel :INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private ObservableCollection<Destination> m_ScrollViewContent;
        
        public Command LocationButtonCommand { get; set; }
        public Command BlueToothButtonCommand { get; set; }
        public Command WifiButtonCommand { get; set; }
        
        private List<Destination> Destinations { get; set; }
        
        #region Constructor
        
        public DefinedDestinationsViewModel()
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
        
        private void onLocationButtonPressed(object obj)
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
            
            IsLocationButtonSelected = true;
            IsWifiButtonSelected = false;
            IsBluetoothButtonSelected = false;
            
            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onWifiButtonPressed(object obj)
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (destination.IsDynamic)
                    continue;
                
                ScrollViewContent.Add(new Destination(destination.Name)
                {
                    SSID = destination.SSID
                });
            }
            
            IsLocationButtonSelected = false;
            IsWifiButtonSelected = true;
            IsBluetoothButtonSelected = false;
            
            OnPropertyChanged(nameof(ScrollViewContent));
        }
        
        private void onBlueToothButtonPressed(object obj)
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (destination.IsDynamic)
                    continue;
                
                ScrollViewContent.Add(new Destination(destination.Name)
                {
                    Bluetooth = destination.Bluetooth
                });
            }
            
            IsLocationButtonSelected = false;
            IsWifiButtonSelected = false;
            IsBluetoothButtonSelected = true;
            
            OnPropertyChanged(nameof(ScrollViewContent));
        }
        
        #region Highlight_Chosen_Button
        
        private bool isLocationButtonSelected;
        private bool isBluetoothButtonSelected;
        private bool isWifiButtonSelected;

        public bool IsLocationButtonSelected
        {
            get => isLocationButtonSelected;
            set
            {
                if (SetField(ref isLocationButtonSelected, value))
                {
                    OnPropertyChanged(nameof(IsLocationButtonSelected));
                }
            }
        }

        public bool IsBluetoothButtonSelected
        {
            get => isBluetoothButtonSelected;
            set
            {
                if (SetField(ref isBluetoothButtonSelected, value))
                {
                    OnPropertyChanged(nameof(IsBluetoothButtonSelected));
                }
            }
        }

        public bool IsWifiButtonSelected
        {
            get => isWifiButtonSelected;
            set
            {
                if (SetField(ref isWifiButtonSelected, value))
                {
                    OnPropertyChanged(nameof(IsWifiButtonSelected));
                }
            }
        }
        
        #endregion
        
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