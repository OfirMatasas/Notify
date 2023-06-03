using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Tokens;
using Notify.Azure.HttpClient;
using Notify.Bluetooth;
using Notify.Core;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public class BluetoothSettingsPageViewModel : BaseViewModel
    {
        public Command BackCommand { get; set; }
        public Command UpdateBluetoothSettingsCommand { get; set; }
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;
        public string SelectedLocation { get; set; }
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
    }
}
