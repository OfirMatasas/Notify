using Xamarin.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;
using Notify.Azure.HttpClient;
using Notify.Bluetooth;
using Notify.Core;
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
                    RemoveBluetoothButtonText = $"REMOVE {value} BLUETOOTH";
                    IsRemoveButtonEnabled = true;
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
            bool successfulUpdate;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert("Confirmation", $"Are you sure you want to remove {SelectedLocation} bluetooth destination from preferences?", "Yes", "No");

            if (isConfirmed)
            {
                successfulUpdate = AzureHttpClient.Instance.RemoveDestination(m_SelectedLocation, NotificationType.Bluetooth).Result;
                
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
        
        private string m_RemoveBluetoothButtonText = "PLEASE CHOOSE DESTINATION";
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
