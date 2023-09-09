using Xamarin.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.WiFi;
using Xamarin.Essentials;

namespace Notify.ViewModels
{
    public class WifiSettingsPageViewModel : INotifyPropertyChanged
    {
        #region Constructor
        
        public WifiSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateWifiSettingsCommand = new Command(onUpdateWifiSettingsClicked);
            RemoveWifiDestinationCommand = new Command(onRemoveWifiDestinationClicked);
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

        private string m_SelectedLocation;
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;
        
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
                        RemoveWifiButtonText = $"REMOVE {value} WI-FI";
                        IsRemoveButtonEnabled = true;
                    }
                    else
                    {
                        RemoveWifiButtonText = $"No {m_SelectedLocation} destination defined";
                        IsRemoveButtonEnabled = false;
                    }
                }
            }
        }

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
        
        #region Remove_Destination
        
        public Command RemoveWifiDestinationCommand { get; set; }
        
        private async void onRemoveWifiDestinationClicked()
        {
            bool successfulUpdate;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert("Confirmation", $"Are you sure you want to remove {SelectedLocation} Wi-Fi Destination from preferences?", "Yes", "No");

            if (isConfirmed)
            {
                successfulUpdate = AzureHttpClient.Instance.RemoveDestination(m_SelectedLocation, NotificationType.WiFi).Result;
                
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
        
        private string m_RemoveWifiButtonText = "CHOOSE DESTINATION";
        public string RemoveWifiButtonText
        {
            get => m_RemoveWifiButtonText;
            set => SetField(ref m_RemoveWifiButtonText, value);
        }

        private bool m_IsRemoveButtonEnabled;
        public bool IsRemoveButtonEnabled
        {
            get => m_IsRemoveButtonEnabled;
            set => SetField(ref m_IsRemoveButtonEnabled, value);
        }
        
        #endregion
    }
}
