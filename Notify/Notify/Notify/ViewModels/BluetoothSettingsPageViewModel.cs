using Xamarin.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Bluetooth;
using Notify.Core;
using Xamarin.Essentials;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public class BluetoothSettingsPageViewModel : INotifyPropertyChanged
    {
        public Command BackCommand { get; set; }
        public Command UpdateBluetoothSettingsCommand { get; set; }
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;
        private string m_SelectedLocation;
        public string SelectedBluetoothID { get; set; }
        public ObservableCollection<string> BluetoothSelectionList { get; set; }
        private BluetoothManager m_BluetoothManager;

        public BluetoothSettingsPageViewModel()
        {
            initCommands();
            initBluetoothManager();
        }

        private void initCommands()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateBluetoothSettingsCommand = new Command(onUpdateBluetoothSettingsClicked);
            RemoveBluetoothDestinationCommand = new Command(onRemoveBluetoothDestinationClicked);
        }

        private void initBluetoothManager()
        {
            m_BluetoothManager = BluetoothManager.Instance;
            BluetoothSelectionList = BluetoothManager.BluetoothSelectionList;
        }

        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_SETTINGS);
        }
        
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
                        RemoveBluetoothButtonText = $"REMOVE {value} BLUETOOTH";
                        IsRemoveButtonEnabled = true;
                    }
                    else
                    {
                        RemoveBluetoothButtonText = $"No {m_SelectedLocation} destination defined";
                        IsRemoveButtonEnabled = false;
                    }
                }
            }
        }

        private async void onUpdateBluetoothSettingsClicked()
        {
            bool successfulUpdate;

            if (SelectedLocation.IsNullOrEmpty() || SelectedBluetoothID.IsNullOrEmpty())
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please select a location and a BT device", "OK");
            }
            else
            {
                successfulUpdate = AzureHttpClient.Instance.UpdateDestination(SelectedLocation, SelectedBluetoothID, NotificationType.Bluetooth)
                    .Result;

                if (successfulUpdate)
                {
                    App.Current.MainPage.DisplayAlert("Update",
                        $"Updated {SelectedBluetoothID} as your {SelectedLocation}", "OK");
                    await AzureHttpClient.Instance.GetDestinations();
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Error", "Something went wrong", "OK");
                }
            }
        }
        
        #region Remove_Destination
        
        public Command RemoveBluetoothDestinationCommand { get; set; }
        
        private async void onRemoveBluetoothDestinationClicked()
        {
            bool isSucceeded;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert("Confirmation", $"Are you sure you want to remove the bluetooth device from your {SelectedLocation} destination?", "Yes", "No");

            if (isConfirmed)
            {
                isSucceeded = AzureHttpClient.Instance
                    .RemoveDestination(m_SelectedLocation, NotificationType.Bluetooth).Result;

                if (isSucceeded)
                {
                    App.Current.MainPage.DisplayAlert("Remove", $"Removal of bluetooth device from {SelectedLocation} succeeded successfully", "OK");
                    await AzureHttpClient.Instance.GetDestinations();
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Error", "Something went wrong", "OK");
                }
            }
        }
        
        private string m_RemoveBluetoothButtonText = "CHOOSE DESTINATION";
        public string RemoveBluetoothButtonText
        {
            get => m_RemoveBluetoothButtonText;
            set => SetField(ref m_RemoveBluetoothButtonText, value);
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
