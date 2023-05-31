using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Notify.Helpers;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Notify.Bluetooth
{
    public class BluetoothManager
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private IBluetoothLE m_BluetoothLE;
        private IAdapter m_BluetoothAdapter;
        public ObservableCollection<string> BluetoothSelectionList { get; }

        public BluetoothManager()
        {
            m_BluetoothLE = CrossBluetoothLE.Current;
            m_BluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            BluetoothSelectionList = new ObservableCollection<string>();

            m_BluetoothLE.StateChanged += onBluetoothStateChanged;
        }

        private async void onBluetoothStateChanged(object sender, BluetoothStateChangedArgs bluetoothState)
        {
            r_Logger.LogInformation(
                $"Switching from {bluetoothState.OldState} to {bluetoothState.NewState}");

            if (bluetoothState.NewState == BluetoothState.On)
            {
                await StartBluetoothScanning();
            }
        }

        public async void startScanningForDevices()
        {
            try
            {
                BluetoothSelectionList.Clear();
                
                if (m_BluetoothLE.IsOn)
                {
                    await StartBluetoothScanning();
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Bluetooth Off",
                        "Bluetooth is currently off. Please turn it on.", "OK");
                    BluetoothSelectionList.Clear();
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred while starting Bluetooth scanning: {ex.Message}");
            }
        }
        
        public async void stopScanningForDevices()
        {
            try
            {
                BluetoothSelectionList.Clear();
                r_Logger.LogInformation("Stop scanning for Bluetooth devices");
                await m_BluetoothAdapter.StopScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred while stopping Bluetooth scanning: {ex.Message}");
            }
        }
        
        private async Task StartBluetoothScanning()
        {
            m_BluetoothAdapter.ScanMode = ScanMode.Balanced;
            m_BluetoothAdapter.ScanTimeout = Constants.HOUR_IN_MS;

            r_Logger.LogInformation(
                $"start scanning for Bluetooth devices. Scan timeout: {TimeSpan.FromMilliseconds(m_BluetoothAdapter.ScanTimeout).Hours} Hours");

            m_BluetoothAdapter.DeviceDiscovered += (sender, deviceArg) =>
            {
                if (!deviceArg.Device.Name.IsNullOrEmpty() && !BluetoothSelectionList.Contains(deviceArg.Device.Name))
                {
                    BluetoothSelectionList.Add(deviceArg.Device.Name);
                    r_Logger.LogInformation($"device added to list: {deviceArg.Device.Name}");
                }
            };

            await m_BluetoothAdapter.StartScanningForDevicesAsync();
        }
    }
}
