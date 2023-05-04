using Notify.HttpClient;
using Xamarin.Forms;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Notify.Azure.HttpClient;

namespace Notify.ViewModels
{
    public class LocationSettingsPageViewModel : BaseViewModel
    {
        private string m_Destination;
        private string m_Longitude;
        private string m_Latitude;
        private string m_SearchText;
        private string m_SelectedAddress;
        private List<string> m_DropBoxSuggestions;
        
        public Command BackCommand { get; set; }
        public Command UpdateLocationCommand { get; set; }
        public Command GetAddressSuggestionsCommand { get; set; }
        public Command GetGeographicCoordinatesCommand { get; set; }
        public Command GetCurrentLocationGeographicCoordinatesCommand { get; set; }
        

        public LocationSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateHomeLocationButtonClicked);
            GetAddressSuggestionsCommand = new Command(onGetAddressSuggestionsButtonClicked);
            GetGeographicCoordinatesCommand = new Command(onGetGeographicCoordinatesButtonClicked);
            GetCurrentLocationGeographicCoordinatesCommand = new Command(onGetCurrentLocationGeographicCoordinatesButtonClicked);
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
                m_Destination = value;
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
                m_SelectedAddress = value;
                OnPropertyChanged(nameof(SelectedAddress));
            }
        }
        
        public string SearchText
        {
            get => m_SearchText;
            set => SetProperty(ref m_SearchText, value);
        }
        
        public List<string> DropBoxOptions
        {
            get { return m_DropBoxSuggestions; }
            set 
            { 
                m_DropBoxSuggestions = value;
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
            Debug.WriteLine($"Getting geographic coordinates for: {SelectedAddress}");
            
            if (string.IsNullOrEmpty(SelectedAddress))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please choose an address!", "OK");
            }
            else
            {
                GoogleHttpClient.Coordinates coordinates = await GoogleHttpClient.GetCoordinatesFromAddress(SelectedAddress);
            
                double longitude = coordinates.Lat;
                double latitude = coordinates.Lng;
                Debug.WriteLine($"Longitude: {longitude}, Latitude: {latitude}");
                
                Longitude = longitude.ToString();
                Latitude = latitude.ToString();
            }
        }

        private async void onGetCurrentLocationGeographicCoordinatesButtonClicked()
        {
            //TODO: This implementation is a place holder.
            double longitude = 0;
            double latitude = 0;
            
            Longitude = longitude.ToString();
            Latitude = latitude.ToString();
        }
        
        public async void onGetAddressSuggestionsButtonClicked()
        {
            Debug.WriteLine($"Getting suggestions for: {m_SearchText}");
            
            DropBoxOptions = await GoogleHttpClient.GetAddressSuggestions(m_SearchText);
        }
    }
}