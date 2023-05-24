using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Net;
using Android.Net.Wifi;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Notifications;
using Notify.WiFi;
using Xamarin.Essentials;
using Xamarin.Forms;
using Application = Android.App.Application;
using Context = Android.Content.Context;
using Debug = System.Diagnostics.Debug;
using Notification = Notify.Core.Notification;

[assembly: Dependency(typeof(Notify.Droid.Managers.AndroidWiFiManager))]
namespace Notify.Droid.Managers
{
    public class AndroidWiFiManager : IWiFiManager
    {
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
        
                if (ssid == m_AndroidWiFi)
                {
                    Debug.WriteLine($"You have just connected to your wifi network: {ssid}!");
                }
                else
                {
                    Debug.WriteLine($"Error with ssid: SSID: {ssid} \nPre define SSID: {m_AndroidWiFi}");
                }
            }
            else
            {
                Debug.WriteLine("Disconnected from wifi network!");
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
            
            if (checkIfTheDeviceIsConnectedToWiFi())
            {
                wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
                ssid = wifiManager.ConnectionInfo.SSID.Trim('"');
                
                notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
                destinationJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
                
                if (!notificationsJson.Equals(string.Empty) && !destinationJson.Equals(string.Empty))
                {
                    notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                    destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationJson);
                    
                    sendAllRelevantWiFiNotifications(destinations, ssid, notifications);
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
                Debug.WriteLine("Sending notifications");

                foreach (Destination destination in destinations)
                {
                    if (destination.SSID.Equals(ssid))
                    {
                        Debug.WriteLine($"Found a destination with SSID of {ssid}");

                        foreach (Notification notification in notifications)
                        {
                            if (notification.Type.Equals(NotificationType.Location) &&
                                notification.TypeInfo.Equals(destination.Name) &&
                                notification.Status.ToLower().Equals("new"))
                            {
                                notification.Status = "Sent";
                                Debug.WriteLine(
                                    $"Sending notification with name: {notification.Name} and description: {notification.Description}");
                                DependencyService.Get<INotificationManager>()
                                    .SendNotification(notification.Name, notification.Description);

                            }
                        }
                    }
                }

                Debug.WriteLine("Finished sending notifications");
                Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            }
        }

        private bool checkIfTheDeviceIsConnectedToWiFi()
        {
            bool isConnectedToWiFi;
            
            ConnectivityManager connectivityManager =
                (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            NetworkCapabilities capabilities =
                connectivityManager.GetNetworkCapabilities(connectivityManager.ActiveNetwork);
            
            isConnectedToWiFi = capabilities.HasTransport(TransportType.Wifi);
            Debug.WriteLine($"Connected to Wi-Fi: {isConnectedToWiFi}");
            
            return isConnectedToWiFi;
        }
    }
}
