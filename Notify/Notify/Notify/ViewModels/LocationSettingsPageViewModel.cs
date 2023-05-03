using Xamarin.Forms;
using System.Diagnostics;
using System;
using Notify.Azure.HttpClient;

namespace Notify.ViewModels
{
    public class LocationSettingsPageViewModel : BaseViewModel
    {
        private string m_Destination;
        private string m_Longitude;
        private string m_Latitude;
        
        public Command BackCommand { get; set; }
        public Command UpdateLocationCommand { get; set; }

        public LocationSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateLocationCommand = new Command(onUpdateHomeLocationButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }
        
        public string Destination
        {
            get => m_Destination;
            set => SetProperty(ref m_Destination, value);
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
    }
}