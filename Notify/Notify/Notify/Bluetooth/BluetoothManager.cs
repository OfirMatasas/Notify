using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Notifications;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.Bluetooth
{
    public class BluetoothManager
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private static BluetoothManager m_Instance;
        private static readonly object r_Lock = new object();

        private IBluetoothLE m_BluetoothLE;
        private IAdapter m_BluetoothAdapter;
        private static object m_NotificationsLock = new object();

        public static ObservableCollection<string> BluetoothSelectionList { get; private set; }

        private BluetoothManager()
        {
            initBluetoothManager();
            subscribeBluetoothEvents();
            StartBluetoothScanning();
        }

        public static BluetoothManager Instance
        {
            get
            {
                if (m_Instance is null)
                {
                    lock (r_Lock)
                    {
                        if (m_Instance is null)
                        {
                            m_Instance = new BluetoothManager();
                        }
                    }
                }
                
                return m_Instance;
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
            if (e.NewState.Equals(BluetoothState.On))
                await StartBluetoothScanning();
            else if (e.NewState.Equals(BluetoothState.Off))
                stopScanningForDevices();
        }

        private async void stopScanningForDevices()
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
                m_BluetoothAdapter.ScanTimeout = Int32.MaxValue;

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
                sendNotifications();
            }
        }

        private void sendNotifications()
        {
            string notificationsJson, destinationJson;
            List<Notification> notifications;
            List<Destination> destinations;
            
            notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            destinationJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);

            if (!notificationsJson.Equals(string.Empty) && !destinationJson.Equals(string.Empty))
            {
                notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationJson);

                sendAllRelevantBluetoothNotifications(destinations, notifications);
            }
        }

        private static void sendAllRelevantBluetoothNotifications(List<Destination> destinations, List<Notification> notifications)
        {
            List<Notification> sentNotifications = new List<Notification>();
            
            lock (m_NotificationsLock)
            {
                r_Logger.LogDebug("Sending notifications");

                foreach (Destination destination in destinations)
                {
                    if (BluetoothSelectionList.Contains(destination.Bluetooth))
                    {
                        r_Logger.LogDebug(
                            $"Found a destination with name: {destination.Name} and bluetooth: {destination.Bluetooth}");

                        foreach (Notification notification in notifications)
                        {
                            if (notification.Type.Equals(NotificationType.Location) &&
                                notification.TypeInfo.Equals(destination.Name) &&
                                notification.Status.ToLower().Equals("new"))
                            {
                                notification.Status = "Sent";
                                r_Logger.LogDebug(
                                    $"Sending notification with name: {notification.Name} and description: {notification.Description}");
                                DependencyService.Get<INotificationManager>()
                                    .SendNotification(notification.Name, notification.Description);
                                sentNotifications.Add(notification);
                            }
                        }
                    }
                }

                r_Logger.LogDebug("Finished sending notifications");
                Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
                updateStatusOfSentNotifications(sentNotifications);
            }
        }

        private static void updateStatusOfSentNotifications(List<Notification> arrivedLocationNotifications)
        {
            string json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(json);

            notifications.ForEach(notification =>
            {
                if (arrivedLocationNotifications.Any(arrivedNotification => arrivedNotification.ID.Equals(notification.ID)))
                {
                    notification.Status = "Sent";
                    r_Logger.LogDebug($"Updated status of notification {notification.ID} to 'Sent'");
                }
            });

            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            AzureHttpClient.Instance.UpdateNotificationsStatus(arrivedLocationNotifications, "Sent");
        }
    }
}