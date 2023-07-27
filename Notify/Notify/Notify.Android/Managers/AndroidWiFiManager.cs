using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Net;
using Android.Net.Wifi;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Notifications;
using Notify.Services;
using Notify.WiFi;
using Xamarin.Essentials;
using Xamarin.Forms;
using Application = Android.App.Application;
using Context = Android.Content.Context;
using Notification = Notify.Core.Notification;

[assembly: Dependency(typeof(Notify.Droid.Managers.AndroidWiFiManager))]
namespace Notify.Droid.Managers
{
    public class AndroidWiFiManager : IWiFiManager
    {
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private readonly string m_AndroidWiFi = "\"AndroidWifi\"";
        private static readonly object m_NotificationsLock = new object();

        public AndroidWiFiManager()
        {
            retrieveDestinations();
        }        
        
        public void PrintConnectedWiFi(object sender, ConnectivityChangedEventArgs e)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            NetworkCapabilities capabilities = connectivityManager.GetNetworkCapabilities(connectivityManager.ActiveNetwork);

            if (capabilities.HasTransport(TransportType.Wifi))
            {
                WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
                string ssid = wifiManager.ConnectionInfo.SSID;
                
                r_Logger.LogInformation($"Connected to Wi-Fi network: {ssid}");
            }
            else
            {
                r_Logger.LogInformation("Disconnected from Wi-Fi network!");
            }
        }

        public List<string> GetAvailableNetworks()
        {
            List<string> WiFiNetworks = new List<string>();
            WifiManager WiFiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
            IList<ScanResult> wifiList = WiFiManager.ScanResults;

            foreach (ScanResult scanResult in wifiList)
            {
                WiFiNetworks.Add(scanResult.Ssid);
            }
            
            return WiFiNetworks;
        }

        public void SendNotifications(object sender, ConnectivityChangedEventArgs connectivityChangedEventArgs)
        {
            WifiManager wifiManager;
            string notificationsJson, ssid, destinationJson;
            List<Notification> notifications;
            List<Destination> destinations;
            
            notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            destinationJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);

            if (!notificationsJson.Equals(string.Empty) && !destinationJson.Equals(string.Empty))
            {
                notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationJson);
                
                if (checkIfConnectedToWiFi())
                {
                    wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
                    ssid = wifiManager.ConnectionInfo.SSID.Trim('"');
                    
                    sendAllRelevantWiFiNotifications(destinations, ssid, notifications);
                }
                else
                {
                    sendNotificationsForLeaveDestinations(notifications, destinations);
                }
            }
        }
        
        private async void retrieveDestinations()
        {
            List<Destination> destinations;
            
            await Task.Run(() =>
            {
                destinations = AzureHttpClient.Instance.GetDestinations().Result;
            });
        }

        private static void sendAllRelevantWiFiNotifications(List<Destination> destinations, string ssid, List<Notification> notifications)
        {
            lock (m_NotificationsLock)
            {
                r_Logger.LogInformation("Sending Wi-Fi notifications");
                
                List<Notification> locationNotifications = notifications.FindAll(notification => notification.Type.Equals(NotificationType.Location));
                List<Destination> sameSSIDDestinations = destinations.FindAll(destination => ssid.Equals(destination.SSID));
                List<Destination> differentSSIDDestinations = destinations.Except(sameSSIDDestinations).ToList();

                sendNotificationsForArrivalDestinations(locationNotifications, sameSSIDDestinations);
                sendNotificationsForLeaveDestinations(locationNotifications, differentSSIDDestinations);

                r_Logger.LogInformation("Finished sending Wi-Fi notifications");
                Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            }
        }
        
        private static void sendNotificationsForArrivalDestinations(List<Notification> notifications, List<Destination> destinations)
        {
            bool isDestinationNotification, isArrivalNotification, isActive;

            r_Logger.LogInformation("Sending Wi-Fi notifications for arrival destinations");

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
                        }
                        else
                        {
                            notification.Status = Constants.NOTIFICATION_STATUS_ARRIVED;
                        }
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending Wi-Fi notifications for arrival destinations");
        }

        private static void sendNotificationsForLeaveDestinations(List<Notification> notifications, List<Destination> destinations)
        {
            bool isDestinationNotification, isLeaveNotification, isArrived;

            r_Logger.LogInformation("Sending Wi-Fi notifications for leave destinations");

            foreach (Destination destination in destinations)
            {
                foreach (Notification notification in notifications)
                {
                    isDestinationNotification = notification.TypeInfo.Equals(destination.Name);
                    isLeaveNotification = notification.Activation.Equals(Constants.NOTIFICATION_ACTIVATION_LEAVE);
                    isArrived = notification.Status.Equals(Constants.NOTIFICATION_STATUS_ARRIVED);

                    if (isDestinationNotification && isLeaveNotification && isArrived)
                    {
                        r_Logger.LogInformation($"Sending notification for leave notification: {notification.Name}");
                        DependencyService.Get<INotificationManager>().SendNotification(notification);
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending Wi-Fi notifications for leave destinations");
        }

        private bool checkIfConnectedToWiFi()
        {
            bool isConnectedToWiFi;
            
            ConnectivityManager connectivityManager =
                (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            NetworkCapabilities capabilities =
                connectivityManager.GetNetworkCapabilities(connectivityManager.ActiveNetwork);
            
            isConnectedToWiFi = capabilities.HasTransport(TransportType.Wifi);
            r_Logger.LogInformation($"Connected to Wi-Fi: {isConnectedToWiFi}");
            
            return isConnectedToWiFi;
        }
    }
}
