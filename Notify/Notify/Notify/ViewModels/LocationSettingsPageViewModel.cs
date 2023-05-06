using Notify.HttpClient;
using Xamarin.Forms;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Notify.Azure.HttpClient;
using Xamarin.Essentials;

namespace Notify.ViewModels
{
    public class LocationSettingsPageViewModel : BaseViewModel
    {
        private string m_Destination;
        private string m_Longitude;
        private string m_Latitude;
        private string m_SearchedAddress;
        private string m_SelectedAddress;
        private List<string> m_DropBoxSuggestions;
        
        public Command BackCommand { get; set; }
        public Command UpdateLocationCommand { get; set; }
        public Command GetAddressSuggestionsCommand { get; set; }
        public Command GetGeographicCoordinatesCommand { get; set; }
        public Command GetCurrentLocationCommand { get; set; }
        
        public LocationSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateHomeLocationButtonClicked);
            GetAddressSuggestionsCommand = new Command(onGetAddressSuggestionsButtonClicked);
            GetGeographicCoordinatesCommand = new Command(onGetGeographicCoordinatesButtonClicked);
            GetCurrentLocationCommand = new Command(onGetCurrentLocationButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }
        
        public string Destination
        {
            get => m_Destination;
            set
            {
                SetProperty(ref m_Destination, value); 
                OnPropertyChanged(nameof(m_Destination));
            }
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
        
        public string SelectedAddress
        {
            get { return m_SelectedAddress; }
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
            get { return m_DropBoxSuggestions; }
            set 
            { 
                SetProperty(ref m_DropBoxSuggestions, value);
                OnPropertyChanged(nameof(DropBoxOptions));
            }
        }
        
        private async void onUpdateHomeLocationButtonClicked()
        {
            double longitude, latitude;
            bool successfulUpdate;

            if (string.IsNullOrEmpty(m_Destination))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please choose destination", "OK");
            }
            else
            {
                if (double.TryParse(m_Longitude, out longitude) && double.TryParse(m_Latitude, out latitude))
                {
                    if (latitude >= -90.0 && latitude <= 90.0 && longitude >= -180.0 && longitude <= 180.0)
                    {
                        Debug.WriteLine($"Both longitude and latitude are in the right range - updating location.{Environment.NewLine}"); 
                        successfulUpdate = AzureHttpClient.Instance.updateDestination(m_Destination, new Core.Location(latitude, longitude));
                        
                        if (successfulUpdate)
                            await App.Current.MainPage.DisplayAlert("Location Updated", "Your location has been updated successfully", "OK");
                        else
                            await App.Current.MainPage.DisplayAlert("Error", "An error occurred while communicating with the server", "OK");
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Longitude and latitude are not in the right range!", "OK");
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Longitude and latitude must be numbers", "OK");
                }
            }
        }

        private async void onGetGeographicCoordinatesButtonClicked()
        {
            GoogleHttpClient.Coordinates coordinates;
            double longitude, latitude;
            
            Debug.WriteLine($"Getting geographic coordinates for: {SelectedAddress}");
            
            if (string.IsNullOrEmpty(SelectedAddress))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please choose an address.", "OK");
            }
            else
            {
                coordinates = await GoogleHttpClient.GetCoordinatesFromAddress(SelectedAddress);
            
                longitude = coordinates.Lng;
                latitude = coordinates.Lat;
                Debug.WriteLine($"onGetGeographicCoordinatesButtonClicked - Longitude: {longitude}, Latitude: {latitude}");
                
                Longitude = longitude.ToString();
                Latitude = latitude.ToString();
            }
        }

         private async void onGetCurrentLocationButtonClicked()
         {
             GeolocationRequest request;
             Location location;
             double longitude, latitude;
        
             try
             {
                 request = new GeolocationRequest(GeolocationAccuracy.High);
                 location = await Geolocation.GetLocationAsync(request);
                 
                 longitude = location.Longitude;
                 latitude = location.Latitude;
        
                 Debug.WriteLine($"onGetCurrentLocationButtonClicked - Current Longitude: {longitude.ToString()}, Latitude: {latitude.ToString()}" );
                 SelectedAddress =  await GoogleHttpClient.GetAddressFromCoordinatesAsync(latitude, longitude);
             }
             catch (Exception ex)
             {
                 Debug.WriteLine($"Error occured on onGetCurrentLocationGeographicCoordinatesButtonClicked: {Environment.NewLine}{ex.Message}");
             }
         }
        
        public async void onGetAddressSuggestionsButtonClicked()
        {
            Debug.WriteLine($"Getting suggestions for: {SearchAddress}");
            
            DropBoxOptions = await GoogleHttpClient.GetAddressSuggestions(SearchAddress);
        }
    }
}