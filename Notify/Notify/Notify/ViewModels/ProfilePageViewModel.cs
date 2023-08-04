using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        //public Command LoadProfilePictureCommand { get; set; }

        private string destinationsJson;
        private List<Destination> Destinations { get; set; }
        private ObservableCollection<string> _scrollViewContent;
        public ObservableCollection<string> ScrollViewContent
        {
            get => _scrollViewContent;
            set
            {
                _scrollViewContent = value;
                OnPropertyChanged(nameof(ScrollViewContent));
            }
        }        
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
            //LoadProfilePictureCommand = new Command(onLoadProfilePicture);
            
            ScrollViewContent = new ObservableCollection<string>();
            
            destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, String.Empty);
            Destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
        }

        // private async void onLoadProfilePicture()
        // {
        //     Debug.WriteLine("Load profile picture");
        //     string action = await App.Current.MainPage.DisplayActionSheet("Profile Picture", "Cancel", null, "Upload New Picture", "Clear picture");
        //     if (action == "Upload new picture")
        //     {
        //         uploadNewProfilePicture();
        //     }
        //     else if (action == "Clear picture")
        //     {
        //         clearProfilePicture();
        //     }
        // }

        private void uploadNewProfilePicture()
        {
            throw new NotImplementedException();
        }

        private void clearProfilePicture()
        {
            throw new NotImplementedException();
        }

        private void onLocationButtonPressed()
        {
            ScrollViewContent.Clear();
            IEnumerable<string> destinationNames = Destinations.Select(d => d.Name);
            foreach (var name in destinationNames)
            {
                ScrollViewContent.Add(name);
                Debug.WriteLine("Added " + name + " to ScrollViewContent");
            }
            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onBlueToothButtonPressed()
        {
            ScrollViewContent.Clear();
            ScrollViewContent.Add("Bluetooth Data 1");
            ScrollViewContent.Add("Bluetooth Data 2");
        }

        private void onWifiButtonPressed()
        {
            ScrollViewContent.Clear();
            ScrollViewContent.Add("Wifi Data 1");
            ScrollViewContent.Add("Wifi Data 2");
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
