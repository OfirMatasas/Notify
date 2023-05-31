using System;
using Xamarin.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.Generic;
using System.Linq;
using Notify.Helpers;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Tokens;
using Notify.Azure.HttpClient;
using Notify.Bluetooth;
using Plugin.BLE.Abstractions.EventArgs;

namespace Notify.ViewModels
{
    public class BluetoothSettingsPageViewModel : BaseViewModel
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        public Command BackCommand { get; set; }
        public Command UpdateBluetoothSettingsCommand { get; set; }
        public List<string> LocationSelectionList { get; set; } = Constants.LOCATIONS_LIST;
        public string SelectedLocation { get; set; }
        public string SelectedBluetoothID { get; set; }
        private BluetoothManager m_BluetoothManager;
        public ObservableCollection<string> BluetoothSelectionList { get; set; }

        public BluetoothSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateBluetoothSettingsCommand = new Command(onUpdateBluetoothSettingsClicked);
            m_BluetoothManager = new BluetoothManager();
            BluetoothSelectionList = m_BluetoothManager.BluetoothSelectionList;

            foreach (string item in m_BluetoothManager.BluetoothSelectionList)
            {
                BluetoothSelectionList.Add(item);
            }

            m_BluetoothManager.m_BluetoothAdapter.DeviceDiscovered += OnDeviceDiscovered;
        }

        private async void OnDeviceDiscovered(object sender, DeviceEventArgs deviceArg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!deviceArg.Device.Name.IsNullOrEmpty() && !BluetoothSelectionList.Contains(deviceArg.Device.Name))
                {
                    BluetoothSelectionList.Add(deviceArg.Device.Name);
                    r_Logger.LogInformation($"device added to list: {deviceArg.Device.Name}");
                }
            });
        }

        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
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
                successfulUpdate = AzureHttpClient.Instance.UpdateDestination(SelectedLocation, SelectedBluetoothID)
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

