using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Services;
using Xamarin.Essentials;
using Location = Notify.Core.Location;

namespace Notify.ViewModels
{
    public class LocationSettingsPageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;

        #region Constructor
        
        public LocationSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateLocationButtonClicked);
            GetAddressSuggestionsCommand = new Command(onGetAddressSuggestionsButtonClicked);
            RemoveLocationDestinationCommand = new Command(onRemoveLocationDestinationClicked);
        }

        #endregion

        #region Location_Selection
        
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;
        
        private string m_SelectedLocation;
        public string SelectedLocation
        {
            get => m_SelectedLocation;
            set
            {
                if (SetField(ref m_SelectedLocation, value))
                {
                    string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
                    List<Destination> destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
                    bool isDestinationExists = destinations.Any(destination => destination.Name == m_SelectedLocation);

                    if (isDestinationExists)
                    {
                        RemoveLocationButtonText = $"REMOVE {value} LOCATION";
                        IsRemoveButtonEnabled = true;
                    }
                    else
                    {
                        RemoveLocationButtonText = $"No {m_SelectedLocation} destination defined";
                        IsRemoveButtonEnabled = false;
                    }
                }
            }
        }
        
        public Command UpdateLocationCommand { get; set; }
        
        private string m_Longitude;
        public string Longitude
        {
            get => m_Longitude;
            set => SetField(ref m_Longitude, value);
        }
        
        private string m_Latitude;
        public string Latitude
        {
            get => m_Latitude;
            set => SetField(ref m_Latitude, value);
        }
        
        #endregion

        #region Address

        public Command GetAddressSuggestionsCommand { get; set; }
        private string m_SearchedAddress;
        private string m_SelectedAddress;
        private List<string> m_DropBoxSuggestions;

        public string SelectedAddress
        {
            get => m_SelectedAddress;
            set
            {
                SetField(ref m_SelectedAddress, value);
                OnPropertyChanged(nameof(SelectedAddress));
                onGetGeographicCoordinatesButtonClicked();
                SearchAddress = value;
            }
        }
        
        public string SearchAddress
        {
            get => m_SearchedAddress;
            set { SetField(ref m_SearchedAddress, value); }
        }

        public List<string> DropBoxOptions
        {
            get => m_DropBoxSuggestions;
            set 
            { 
                SetField(ref m_DropBoxSuggestions, value);
                OnPropertyChanged(nameof(DropBoxOptions));
            }
        }
        
        #endregion

        #region Update_Location

        private async void onUpdateLocationButtonClicked()
        {
            double longitude = 0, latitude = 0;
            string errorMessage = string.Empty;

            if (checkValidationOfLocation(ref longitude, ref latitude, ref errorMessage))
            {
                await updateLocation(latitude, longitude);
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
            }
        }

        private async Task updateLocation(double latitude, double longitude)
        {
            Location location = new Location(latitude: latitude, longitude: longitude);
            bool successfulUpdate;
            
            r_Logger.LogDebug("Longitude and latitude are in the right range - updating location");
            successfulUpdate = AzureHttpClient.Instance.UpdateDestination(SelectedLocation, location, NotificationType.Location).Result;

            if (successfulUpdate)
            {
                await App.Current.MainPage.DisplayAlert(
                    title: "Location Updated", 
                    message: $"{SelectedLocation} has been updated successfully", 
                    cancel: "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert(
                    title: "Error", 
                    message: "An error occurred while communicating with the server", 
                    cancel: "OK");
            }
        }

        private bool checkValidationOfLocation(ref double longitude, ref double latitude, ref string errorMessage)
        {
            bool valid = false;
            
            if (string.IsNullOrEmpty(SelectedLocation))
            {
                errorMessage = "Please choose a location";
            }
            else if (!double.TryParse(Longitude, out longitude) || !double.TryParse(Latitude, out latitude))
            {
                errorMessage = "Longitude and latitude must be numbers";
            }
            else if (latitude < Constants.LATITUDE_MIN || latitude > Constants.LATITUDE_MAX || longitude < Constants.LONGITUDE_MIN || longitude > Constants.LONGITUDE_MAX)
            {
                errorMessage = "Longitude and latitude are not in the right range";
            }
            else
            {
                valid = true;
            }

            return valid;
        }
        
        #endregion
        
        #region Back_Button

        public Command BackCommand { get; set; }

        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_SETTINGS);
        }

        #endregion

        #region Use_Current_Location

        private bool m_IsCurrentLocationEnabled;
        private string m_SelectedAddressLongitude;
        private string m_SelectedAddressLatitude;
        public bool IsCurrentLocationEnabled
        {
            get => m_IsCurrentLocationEnabled;
            set
            {
                m_IsCurrentLocationEnabled = value;
                OnPropertyChanged(nameof(IsCurrentLocationEnabled));

                if (m_IsCurrentLocationEnabled)
                {
                    m_SelectedAddressLongitude = Longitude;
                    m_SelectedAddressLatitude = Latitude;
                    EntryBackgroundColor = Color.FromHex("#D1D1D1");
                    getCurrentLocationButtonClicked();
                }
                else
                {
                    Longitude = m_SelectedAddressLongitude;
                    Latitude = m_SelectedAddressLatitude;
                    EntryBackgroundColor = Color.White;
                }
            }
        }
        public bool m_IsOtherLocationEnabled => !m_IsCurrentLocationEnabled;
        
        private Color m_EntryBackgroundColor = Color.White;
        public Color EntryBackgroundColor
        {
            get => m_EntryBackgroundColor;
            set
            {
                m_EntryBackgroundColor = value;
                OnPropertyChanged(nameof(EntryBackgroundColor));
            }
        }
        
        #endregion
        
        private async void onGetGeographicCoordinatesButtonClicked()
        {
            Location location;
            
            r_Logger.LogDebug($"Getting geographic coordinates for: {SelectedAddress}");
            
            if (string.IsNullOrEmpty(SelectedAddress))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please choose an address.", "OK");
            }
            else
            {
                location = await AzureHttpClient.Instance.GetCoordinatesFromAddress(SelectedAddress);
                
                if(!location.Equals(null))
                {
                    Longitude = location.Longitude.ToString();
                    Latitude = location.Latitude.ToString();
                    r_Logger.LogInformation($"onGetGeographicCoordinatesButtonClicked - Longitude: {Longitude}, Latitude: {Latitude}");
                }
                else
                {
                    r_Logger.LogError($"onGetGeographicCoordinatesButtonClicked - No coordinates found for this address.");
                    await App.Current.MainPage.DisplayAlert("Error", "No coordinates found for this address.", "OK");
                }
            }
        }

         private async void getCurrentLocationButtonClicked()
         {
             GeolocationRequest request;
             Xamarin.Essentials.Location location;

             try
             {
                 request = new GeolocationRequest(GeolocationAccuracy.High);
                 location = await Xamarin.Essentials.Geolocation.GetLocationAsync(request);
                 
                 Longitude = location.Longitude.ToString();
                 Latitude = location.Latitude.ToString();
                 r_Logger.LogDebug($"onGetCurrentLocationButtonClicked - Longitude: {Longitude}, Latitude: {Latitude}");
             }
             catch (Exception ex)
             {
                 r_Logger.LogError($"onGetCurrentLocationButtonClicked: {ex.Message}");
             }
         }
        
        private async void onGetAddressSuggestionsButtonClicked()
        {
            r_Logger.LogDebug($"Getting suggestions for {SearchAddress}");

            DropBoxOptions = await AzureHttpClient.Instance.GetAddressSuggestions(SearchAddress);
        }
        
        #region Remove_Destination
        
        public Command RemoveLocationDestinationCommand { get; set; }
        
        private async void onRemoveLocationDestinationClicked()
        {
            bool successfulUpdate;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert("Confirmation", $"Are you sure you want to remove {SelectedLocation} Location destination from preferences?", "Yes", "No");

            if (isConfirmed)
            {
                successfulUpdate = AzureHttpClient.Instance.RemoveDestination(m_SelectedLocation, NotificationType.Location).Result;
                
                if (successfulUpdate)
                {
                    App.Current.MainPage.DisplayAlert("Remove", $"Remove success!", "OK");
                    await AzureHttpClient.Instance.GetDestinations();
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Error", "Something went wrong", "OK");
                }
            }
        }
        
        private string m_RemoveLocationButtonText = "CHOOSE DESTINATION";
        public string RemoveLocationButtonText
        {
            get => m_RemoveLocationButtonText;
            set => SetField(ref m_RemoveLocationButtonText, value);
        }

        private bool m_IsRemoveButtonEnabled;
        public bool IsRemoveButtonEnabled
        {
            get => m_IsRemoveButtonEnabled;
            set => SetField(ref m_IsRemoveButtonEnabled, value);
        }
        
        #endregion
        
        #region Interface_Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}

