using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Helpers;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Notify.Bluetooth
{
    public class BluetoothManager
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private static BluetoothManager m_Instance;
        private static readonly object r_Lock = new object();

        private IBluetoothLE m_BluetoothLE;
        private IAdapter m_BluetoothAdapter;

        public ObservableCollection<string> BluetoothSelectionList { get; private set; }

        private BluetoothManager()
        {
            initBluetoothManager();
            subscribeBluetoothEvents();
        }

        public static BluetoothManager Instance
        {
            get
            {
                lock (r_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = new BluetoothManager();
                    }
                    return m_Instance;
                }
            }
        }

        private void initBluetoothManager()
        {
            m_BluetoothLE = CrossBluetoothLE.Current;
            m_BluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            BluetoothSelectionList = new ObservableCollection<string>();
        }

        private void subscribeBluetoothEvents()
        {
            m_BluetoothLE.StateChanged += onBluetoothStateChanged;
            m_BluetoothAdapter.DeviceDiscovered += onDeviceDiscovered;
        }

        private async void onBluetoothStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            if (e.NewState == BluetoothState.On)
                await StartBluetoothScanning();
            else if (e.NewState == BluetoothState.Off)
                StopScanningForDevices();
        }

        public async Task<bool> CheckBluetoothStatus()
        {
            var isBluetoothOn = m_BluetoothLE.State == BluetoothState.On;
            if (isBluetoothOn)
                await StartBluetoothScanning();
            else
                r_Logger.LogInformation("Bluetooth Off");

            return isBluetoothOn;
        }

        public async void StopScanningForDevices()
        {
            try
            {
                BluetoothSelectionList.Clear();
                r_Logger.LogDebug("Stop scanning for Bluetooth devices");
                await m_BluetoothAdapter.StopScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred while stopping Bluetooth scanning: {ex.Message}");
            }
        }

        public async Task StartBluetoothScanning()
        {
            try
            {
                m_BluetoothAdapter.ScanMode = ScanMode.Balanced;

                r_Logger.LogDebug($"Start scanning for Bluetooth devices");

                await m_BluetoothAdapter.StartScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred in StartBluetoothScanning: {ex.Message}");
            }
        }

        private void onDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Device.Name) && !BluetoothSelectionList.Contains(e.Device.Name))
            {
                BluetoothSelectionList.Add(e.Device.Name);
                r_Logger.LogInformation($"Device added to list: {e.Device.Name}");
            }
        }
    }
}
