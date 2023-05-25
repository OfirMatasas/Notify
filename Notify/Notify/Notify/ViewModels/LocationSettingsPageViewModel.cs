using Notify.HttpClient;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Notify.Azure.HttpClient;
using Notify.Helpers;
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
            GetGeographicCoordinatesCommand = new Command(onGetGeographicCoordinatesButtonClicked);
            GetCurrentLocationCommand = new Command(onGetCurrentLocationButtonClicked);
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
        public Command GetGeographicCoordinatesCommand { get; set; }
        public Command GetCurrentLocationCommand { get; set; }
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
            Location location = new Location(latitude, longitude);
            bool successfulUpdate;
            
            r_Logger.LogDebug("Longitude and latitude are in the right range - updating location");
            successfulUpdate = AzureHttpClient.Instance.UpdateDestination(SelectedLocation, location).Result;

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
            await Shell.Current.GoToAsync("///settings");
        }

        #endregion

        private async void onGetGeographicCoordinatesButtonClicked()
        {
            GoogleHttpClient.Coordinates coordinates;
            
            r_Logger.LogDebug($"Getting geographic coordinates for: {SelectedAddress}");
            
            if (string.IsNullOrEmpty(SelectedAddress))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please choose an address.", "OK");
            }
            else
            {
                coordinates = await GoogleHttpClient.GetCoordinatesFromAddress(SelectedAddress);
                Longitude = coordinates.Lng.ToString();
                Latitude = coordinates.Lat.ToString();
                r_Logger.LogDebug($"onGetGeographicCoordinatesButtonClicked - Longitude: {Longitude}, Latitude: {Latitude}");
            }
        }

         private async void onGetCurrentLocationButtonClicked()
         {
             GeolocationRequest request;
             Xamarin.Essentials.Location location;
             double longitude, latitude;
             
             try
             {
                 request = new GeolocationRequest(GeolocationAccuracy.High);
                 location = await Xamarin.Essentials.Geolocation.GetLocationAsync(request);
                 
                 longitude = location.Longitude;
                 latitude = location.Latitude;
        
                 SelectedAddress =  await GoogleHttpClient.GetAddressFromCoordinatesAsync(latitude, longitude);
                 Longitude = longitude.ToString();
                 Latitude = latitude.ToString();
                 r_Logger.LogDebug($"onGetCurrentLocationButtonClicked - Longitude: {Longitude}, Latitude: {Latitude}");
             }
             catch (Exception ex)
             {
                 r_Logger.LogError($"onGetCurrentLocationButtonClicked: {ex.Message}");
             }
         }
        
        public async void onGetAddressSuggestionsButtonClicked()
        {
            r_Logger.LogDebug($"Getting suggestions for {SearchAddress}");

            DropBoxOptions = await GoogleHttpClient.GetAddressSuggestions(SearchAddress);
        }
    }
}
