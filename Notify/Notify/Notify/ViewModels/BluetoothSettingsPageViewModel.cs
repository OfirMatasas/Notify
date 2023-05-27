using System;
using Xamarin.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.Generic;
using System.Linq;
using Notify.Helpers;
using System.Collections.ObjectModel;

namespace Notify.ViewModels
{
    public class BluetoothSettingsPageViewModel
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        public Command BackCommand { get; set; }
        public Command UpdateBluetoothSettingsCommand { get; set; }
        private List<IDevice> m_ScannedDevices;
        private IBluetoothLE m_BluetoothLE;
        private IAdapter m_BluetoothAdapter;
        public ObservableCollection<string> BluetoothSelectionList { get; }


        public BluetoothSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateBluetoothSettingsCommand = new Command(onUpdateBluetoothSettingsClicked);
            m_ScannedDevices = new List<IDevice>();
            m_BluetoothLE = CrossBluetoothLE.Current;
            m_BluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            BluetoothSelectionList = new ObservableCollection<string>();
            
            scanForDevices();
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }

        private async void onUpdateBluetoothSettingsClicked()
        {
            
        }
        
        private void scanForDevices()
        {
            m_BluetoothAdapter.ScanMode = ScanMode.Balanced;
            m_BluetoothAdapter.ScanTimeout = Constants.ONE_MINUTE_IN_MS;
            
            m_BluetoothLE.StateChanged += async (sender, e) =>
            {
                r_Logger.LogInformation($"Switching from {e.OldState} to {e.NewState}");

                if (e.NewState.Equals(BluetoothState.On))
                {
                    m_ScannedDevices.Clear(); //TODO check if necessary
                    BluetoothSelectionList.Clear();
                    m_BluetoothAdapter.DeviceDiscovered += (s, a) =>
                    {
                        if (m_ScannedDevices.All(d => d.Id != a.Device.Id))
                        {
                            m_ScannedDevices.Add(a.Device);
                            BluetoothSelectionList.Add(a.Device.Name ?? a.Device.Id.ToString());
                            r_Logger.LogInformation($"device added to list: {a.Device.Name} | {a.Device.Id}");
                        }
                    };

                    await m_BluetoothAdapter.StartScanningForDevicesAsync();
                }
                else if (e.NewState.Equals(BluetoothState.Off))
                {
                    m_ScannedDevices.Clear();
                    BluetoothSelectionList.Clear();
                    await m_BluetoothAdapter.StopScanningForDevicesAsync();
                }
            };
        }
    }
}
