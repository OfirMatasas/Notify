using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Notifications;
using Notify.Services;
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
            m_BluetoothAdapter.DeviceConnectionLost += onDeviceConnectionLost;
            m_BluetoothAdapter.DeviceDisconnected += onDeviceConnectionLost;
        }

        private async void onBluetoothStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            if (e.NewState.Equals(BluetoothState.On))
            {
                await StartBluetoothScanning();
            }
            else if (e.NewState.Equals(BluetoothState.Off))
            {
                stopScanningForDevices();
                onDeviceConnectionLost(sender, null);
            }
        }

        private void onDeviceConnectionLost(object sender, EventArgs e)
        {
            string notificationsJson, destinationsJson;
            List<Destination> destinations;
            List<Notification> notifications;
            List<Notification> sentNotifications = new List<Notification>();
            List<Notification> permanentNotifications = new List<Notification>();

            if (e is DeviceEventArgs args)
            {
                BluetoothSelectionList.Remove(args.Device.Name);
            }
            else if (e is DeviceErrorEventArgs errorArgs)
            {
                BluetoothSelectionList.Remove(errorArgs.Device.Name);
            }

            notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);

            if (!notificationsJson.IsNullOrEmpty() && !destinationsJson.IsNullOrEmpty())
            {
                notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);

                sendNotificationsForLeaveDestinations(notifications, destinations, ref sentNotifications, ref permanentNotifications);
                
                Utils.updateNotificationsStatus(sentNotifications, Constants.NOTIFICATION_STATUS_EXPIRED);
                Utils.updateNotificationsStatus(permanentNotifications, Constants.NOTIFICATION_STATUS_ACTIVE);
            }
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
                
                sendNotifications(e.Device.Name);
            }
        }

        private void sendNotifications(string deviceName)
        {
            string notificationsJson, destinationJson;
            List<Notification> notifications;
            List<Destination> destinations;
            
            notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            destinationJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);

            if (!notificationsJson.IsNullOrEmpty() && !destinationJson.IsNullOrEmpty())
            {
                notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationJson);

                sendAllRelevantBluetoothNotifications(destinations, notifications, deviceName);
            }
        }

        private void sendAllRelevantBluetoothNotifications(List<Destination> destinations, List<Notification> notifications, string deviceName)
        {
            lock (m_NotificationsLock)
            {
                r_Logger.LogInformation("Sending bluetooth notifications");
                
                List<Notification> locationNotifications = notifications.FindAll(notification => notification.Type.Equals(NotificationType.Location));
                List<Destination> sameBluetoothDestinations = destinations.FindAll(destination => deviceName.Equals(destination.Bluetooth));
                List<Destination> differentBluetoothDestinations = destinations.Except(sameBluetoothDestinations).ToList();
                List<Notification> sentNotifications = new List<Notification>();
                List<Notification> arrivedNotifications = new List<Notification>();
                List<Notification> permanentNotifications = new List<Notification>();

                sendNotificationsForArrivalDestinations(locationNotifications, sameBluetoothDestinations, ref sentNotifications, ref arrivedNotifications);
                sendNotificationsForLeaveDestinations(locationNotifications, differentBluetoothDestinations, ref sentNotifications, ref permanentNotifications);
                
                r_Logger.LogInformation("Finished sending bluetooth notifications");

                Utils.updateNotificationsStatus(sentNotifications, Constants.NOTIFICATION_STATUS_EXPIRED);
                Utils.updateNotificationsStatus(arrivedNotifications, Constants.NOTIFICATION_STATUS_ARRIVED);
                Utils.updateNotificationsStatus(permanentNotifications, Constants.NOTIFICATION_STATUS_ACTIVE);
            }
        }

        private void sendNotificationsForArrivalDestinations(List<Notification> notifications,
            List<Destination> destinations, ref List<Notification> sentNotifications,
            ref List<Notification> arrivedNotifications)
        {
            bool isDestinationNotification, isArrivalNotification, isActive;

            r_Logger.LogInformation("Sending bluetooth notifications for arrival destinations");
            
            foreach (Destination destination in destinations)
            {
                foreach (Notification notification in notifications)
                {
                    isDestinationNotification = notification.TypeInfo.Equals(destination.Name);
                    isActive = notification.Status.Equals(Constants.NOTIFICATION_STATUS_ACTIVE);
                    isArrivalNotification = notification.Activation.Equals(Constants.NOTIFICATION_ACTIVATION_ARRIVAL);

                    if (isDestinationNotification && isActive)
                    {
                        if (isArrivalNotification)
                        {
                            r_Logger.LogInformation($"Sending notification for arrival notification: {notification.Name}");
                            DependencyService.Get<INotificationManager>().SendNotification(notification);

                            if (notification.IsPermanent)
                            {
                                arrivedNotifications.Add(notification);
                            }
                            else
                            {
                                sentNotifications.Add(notification);
                            }
                        }
                        else
                        {
                            notification.Status = Constants.NOTIFICATION_STATUS_ARRIVED;
                            arrivedNotifications.Add(notification);
                        }
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending bluetooth notifications for arrival destinations");
        }
        
        private void sendNotificationsForLeaveDestinations(List<Notification> notifications,
            List<Destination> destinations, ref List<Notification> sentNotifications, ref List<Notification> permanentNotifications)
        {
            bool isDestinationNotification, isLeaveNotification, isArrived;

            r_Logger.LogInformation("Sending bluetooth notifications for leave destinations");

            foreach (Destination destination in destinations)
            {
                foreach (Notification notification in notifications)
                {
                    isDestinationNotification = notification.TypeInfo.Equals(destination.Name);
                    isLeaveNotification = notification.Activation.Equals(Constants.NOTIFICATION_ACTIVATION_LEAVE);
                    isArrived = notification.Status.Equals(Constants.NOTIFICATION_STATUS_ARRIVED);

                    if (isDestinationNotification && isArrived)
                    {
                        if (isLeaveNotification)
                        {
                            r_Logger.LogInformation($"Sending notification for leave notification: {notification.Name}");
                            DependencyService.Get<INotificationManager>().SendNotification(notification);
                            
                            if(notification.IsPermanent)
                            {
                                permanentNotifications.Add(notification);
                            }
                            else
                            {
                                sentNotifications.Add(notification);
                            }
                        }
                        else if(notification.IsPermanent)
                        {
                            permanentNotifications.Add(notification);
                        }
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending bluetooth notifications for leave destinations");
        }
    }
}
