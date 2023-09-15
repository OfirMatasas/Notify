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
            List<Notification> sentNotifications = new List<Notification>();
            List<Notification> permanentNotifications = new List<Notification>();
            
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
                    sendNotificationsForLeaveDestinations(notifications, destinations, ref sentNotifications, ref permanentNotifications);
                    Utils.UpdateNotificationsStatus(sentNotifications, Constants.NOTIFICATION_STATUS_EXPIRED);
                    Utils.UpdateNotificationsStatus(permanentNotifications, Constants.NOTIFICATION_STATUS_ACTIVE);
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
                List<Notification> sentNotifications = new List<Notification>();
                List<Notification> arrivedNotifications = new List<Notification>();
                List<Notification> permanentNotifications = new List<Notification>();

                sendNotificationsForArrivalDestinations(locationNotifications, sameSSIDDestinations, ref sentNotifications, ref arrivedNotifications);
                sendNotificationsForLeaveDestinations(locationNotifications, differentSSIDDestinations, ref sentNotifications, ref permanentNotifications);

                r_Logger.LogInformation("Finished sending Wi-Fi notifications");
                
                Utils.UpdateNotificationsStatus(sentNotifications, Constants.NOTIFICATION_STATUS_EXPIRED);
                Utils.UpdateNotificationsStatus(arrivedNotifications, Constants.NOTIFICATION_STATUS_ARRIVED);
                Utils.UpdateNotificationsStatus(permanentNotifications, Constants.NOTIFICATION_STATUS_ACTIVE);
            }
        }
        
        private static void sendNotificationsForArrivalDestinations(List<Notification> notifications, List<Destination> destinations, ref List<Notification> sentNotifications, ref List<Notification> arrivedNotifications)
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
                            if (notification.ShouldBeNotified.Equals(notification.Creator))
                            {
                                r_Logger.LogInformation($"Sending newsfeed to creator {notification.Creator} for arrival notification: {notification.Name}");
                                AzureHttpClient.Instance.SendNewsfeed(new Newsfeed(notification.Creator, $"{notification.Target} Arrived {notification.TypeInfo}", $"{notification.Target} has arrived to their {notification.TypeInfo} destination"));
                            }
                            else
                            {
                                r_Logger.LogInformation($"Sending notification for arrival notification: {notification.Name}");
                                DependencyService.Get<INotificationManager>().SendNotification(notification);
                            }
                            
                            if (notification.IsPermanent)
                            {
                                r_Logger.LogInformation($"Notification {notification.Name} is permanent, and it's added to arrived notifications list");
                                arrivedNotifications.Add(notification);
                            }
                            else
                            {
                                r_Logger.LogInformation($"Notification {notification.Name} is not permanent, and it's added to sent notifications list");
                                sentNotifications.Add(notification);
                            }
                        }
                        else
                        {
                            r_Logger.LogInformation($"Notification {notification.Name} is leave notification, and it's added to arrived notifications list");
                            arrivedNotifications.Add(notification);
                        }
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending Wi-Fi notifications for arrival destinations");
        }

        private static void sendNotificationsForLeaveDestinations(List<Notification> notifications, List<Destination> destinations, ref List<Notification> sentNotifications, ref List<Notification> permanentNotifications)
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

                    if (isDestinationNotification && isArrived)
                    {
                        if (isLeaveNotification)
                        {
                            if(notification.ShouldBeNotified.Equals(notification.Creator))
                            {
                                r_Logger.LogInformation($"Sending newsfeed to creator {notification.Creator} for leave notification: {notification.Name}");
                                AzureHttpClient.Instance.SendNewsfeed(new Newsfeed(notification.Creator, $"{notification.Target} Left {notification.TypeInfo}", $"{notification.Target} has left their {notification.TypeInfo} destination"));
                            }
                            else
                            {
                                r_Logger.LogInformation($"Sending notification for leave notification: {notification.Name}");
                                DependencyService.Get<INotificationManager>().SendNotification(notification);
                            }
                            
                            if(notification.IsPermanent)
                            {
                                r_Logger.LogInformation($"Notification {notification.Name} is permanent, and it's added to permanent notifications list");
                                permanentNotifications.Add(notification);
                            }
                            else
                            {
                                r_Logger.LogInformation($"Notification {notification.Name} is not permanent, and it's added to sent notifications list");
                                sentNotifications.Add(notification);
                            }
                        }
                        else if(notification.IsPermanent)
                        {
                            r_Logger.LogInformation($"Notification {notification.Name} is permanent, and it's added to permanent notifications list");
                            permanentNotifications.Add(notification);
                        }
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
