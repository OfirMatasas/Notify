using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.WiFi;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class WifiSettingsPageViewModel : INotifyPropertyChanged
    {
        #region Constructor
        
        public WifiSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateWifiSettingsCommand = new Command(onUpdateWifiSettingsClicked);
        }

        #endregion

        #region Back_Button

        public Command BackCommand { get; set; }

        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_SETTINGS);
        }

        #endregion

        #region Location_Selection
        
        public string SelectedLocation { get; set; }
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;

        #endregion

        #region WiFi_Selection

        public string SelectedWiFiSSID { get; set; }
        public List<string> WiFiSelectionList { get; set; } = DependencyService.Get<IWiFiManager>().GetAvailableNetworks();

        #endregion

        #region Update_Location

        public Command UpdateWifiSettingsCommand { get; set; }
        
        private async void onUpdateWifiSettingsClicked()
        {
            bool successfulUpdate;
            
            if(SelectedLocation.IsNullOrEmpty() || SelectedWiFiSSID.IsNullOrEmpty())
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please select a location and a WiFi network", "OK");
            }
            else
            {
                successfulUpdate =  AzureHttpClient.Instance.UpdateDestination(SelectedLocation, SelectedWiFiSSID, NotificationType.WiFi).Result;
                
                if (successfulUpdate)
                {
                    App.Current.MainPage.DisplayAlert("Update", $"Updated {SelectedWiFiSSID} as your {SelectedLocation}", "OK");
                    await AzureHttpClient.Instance.GetDestinations();
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Error", "Something went wrong", "OK");
                }
            }
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
