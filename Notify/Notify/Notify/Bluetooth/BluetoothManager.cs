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
            
            m_BluetoothLE.StateChanged += OnBluetoothStateChanged;
        }

        private async void OnBluetoothStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            if (e.NewState == BluetoothState.On)
            {
                await StartBluetoothScanning();
            }
            else if (e.NewState == BluetoothState.Off)
            {
                StopScanningForDevices();
            }
        }

        public async Task<bool> CheckBluetoothStatus()
        {
            var bluetoothState = m_BluetoothLE.State;

            if (bluetoothState == BluetoothState.On)
            {
                await StartBluetoothScanning();
                return true;
            }
            else
            {
                logMessage("Bluetooth Off", "Bluetooth is currently off. Please turn it on.");
                return false;
            }
        }

        public async void StopScanningForDevices()
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

        public async Task StartBluetoothScanning()
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

        private void logMessage(string title, string message)
        {
            r_Logger.LogInformation($"DisplayAlert: {title} - {message}");
        }
    }
}
