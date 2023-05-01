using System.Threading.Tasks;
using Notify.HttpClient;
using Xamarin.Forms;
using Notify.HttpClient;
using Xamarin.Essentials;
using System.ComponentModel;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Android.Print;

namespace Notify.ViewModels
{
    public class LocationSettingsPageViewModel : BaseViewModel
    {
        private string m_Destination;
        private string m_Longitude;
        private string m_Latitude;

        private string m_SearchText;
        private List<string> m_DropBoxSuggestions;
        
        public Command BackCommand { get; set; }
        private string m_SelectedItem;

        public Command UpdateLocationCommand { get; set; }
        public Command GetAddressSuggestionsCommand { get; set; }
        public Command GetGeographicCoordinatesCommand { get; set; }
        

        public LocationSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateHomeLocationButtonClicked);
            GetAddressSuggestionsCommand = new Command(onGetAddressSuggestionsButtonClicked);
            GetGeographicCoordinatesCommand = new Command(onGetGeographicCoordinatesButtonClicked);
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
                m_SelectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
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
        public string SelectedItem
        {
            get { return m_SelectedItem; }
            set
            {
                m_SelectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
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
            set { m_DropBoxSuggestions = value; OnPropertyChanged(nameof(DropBoxOptions)); }
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
        
        private async void onGetAddressSuggestionsButtonClicked()
        {
            UpdateDropBoxOptions();
        }
        
        private async void onGetGeographicCoordinatesButtonClicked()
        {
            Debug.WriteLine($"Geographic Coordinates for: {SelectedItem}");
            
            if (string.IsNullOrEmpty(SelectedItem))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please choose address!", "OK");
            }
            else
            {
                GoogleHttpClient.LatLng latLng = await GoogleHttpClient.GetLatLngFromAddress(SelectedItem);   // TODO: change the place where we take the text from
            
                double Lng = latLng.Lat;
                double Lat = latLng.Lng;
                Debug.WriteLine($"Longitude: {Lng}    |     Latitude: {Lat}");
                
                Longitude = Lng.ToString();
                Latitude = Lat.ToString();
            }
        }
        
        public async void UpdateDropBoxOptions()
        {
            Debug.WriteLine($"Suggestions for: {m_SearchText}");
            
            DropBoxOptions = await GoogleHttpClient.GetAddressSuggestions(m_SearchText);
        }
    }
}