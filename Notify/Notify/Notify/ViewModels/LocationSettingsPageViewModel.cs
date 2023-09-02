using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Services;
using Xamarin.Essentials;
using Location = Notify.Core.Location;

namespace Notify.ViewModels
{
    public class LocationSettingsPageViewModel : BaseViewModel
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;

        #region Constructor
        
        public LocationSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateLocationButtonClicked);
            GetAddressSuggestionsCommand = new Command(onGetAddressSuggestionsButtonClicked);
        }

        #endregion

        #region Location_Selection
        
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;
        
        private string m_SelectedLocation;
        public string SelectedLocation
        {
            get => m_SelectedLocation;
            set => SetProperty(ref m_SelectedLocation, value);
        }
        
        public Command UpdateLocationCommand { get; set; }
        
        private string m_Longitude;
        public string Longitude
        {
            get => m_Longitude;
            set => SetProperty(ref m_Longitude, value);
        }
        
        private string m_Latitude;
        public string Latitude
        {
            get => m_Latitude;
            set => SetProperty(ref m_Latitude, value);
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
                SetProperty(ref m_SelectedAddress, value);
                OnPropertyChanged(nameof(SelectedAddress));
                onGetGeographicCoordinatesButtonClicked();
                SearchAddress = value;
            }
        }
        
        public string SearchAddress
        {
            get => m_SearchedAddress;
            set { SetProperty(ref m_SearchedAddress, value); }
        }

        public List<string> DropBoxOptions
        {
            get => m_DropBoxSuggestions;
            set 
            { 
                SetProperty(ref m_DropBoxSuggestions, value);
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

        private bool m_IsUseCurrentLocation;
        private string m_TmpLongitude;
        private string m_TmpLatitude;
        public bool IsUseCurrentLocation
        {
            get => m_IsUseCurrentLocation;
            set
            {
                m_IsUseCurrentLocation = value;
                OnPropertyChanged(nameof(IsUseCurrentLocation));

                if (m_IsUseCurrentLocation)
                {
                    m_TmpLongitude = Longitude;
                    m_TmpLatitude = Latitude;
                    getCurrentLocationButtonClicked();
                }
                else
                {
                    Longitude = m_TmpLongitude;
                    Latitude = m_TmpLatitude;
                }
            }
        }
        public bool m_IsUseOtherLocation => !m_IsUseCurrentLocation;
        
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
    }
}
