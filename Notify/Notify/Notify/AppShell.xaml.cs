using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Bluetooth;
using Notify.Core;
using Notify.Helpers;
using Notify.Notifications;
using Notify.Services;
using Notify.WiFi;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Location = Notify.Core.Location;

namespace Notify
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell
    {
        private readonly INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        private readonly IWiFiManager m_WiFiManager = DependencyService.Get<IWiFiManager>();
        private static readonly object m_NotificationsLock = new object();
        private static readonly object m_InitializeLock = new object();
        private static bool m_IsInitialized;
        private BluetoothManager m_BluetoothManager;

        public AppShell()
        {
            InitializeComponent();
            initializeAppShell();
        }

        private void initializeAppShell()
        {
            lock (m_InitializeLock)
            {
                if (!m_IsInitialized)
                {
                    m_IsInitialized = true;
                    Connectivity.ConnectivityChanged += internetConnectivityChanged;
                    m_BluetoothManager = BluetoothManager.Instance;
                    m_BluetoothManager.StartBluetoothScanning();
                    setNoficicationManagerNotificationReceived();
                    setMessagingCenterSubscriptions();
                    
                    if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
                    {
                        startService();
                    }

                    retriveDestinations();
                }
            }
        }
        
        private void internetConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            m_WiFiManager.PrintConnectedWiFi(sender, e);
            m_WiFiManager.SendNotifications(sender, e);
        }

        private void setMessagingCenterSubscriptions()
        {
            setMessagingCenterLocationSubscription();
            setMessagingCenterStopServiceMessageSubscription();
            setMessagingCenterLocationErrorMessageSubscription();
            setMessagingCenterLocationArrivedMessageSubscription();
        }

        private async void retriveDestinations()
        {
            await Task.Run(() =>
            {
                List<Destination> destinations = AzureHttpClient.Instance.GetDestinations().Result;
            });
        }

        private void setMessagingCenterLocationArrivedMessageSubscription()
        {
            MessagingCenter.Subscribe<LocationArrivedMessage>(this, "LocationArrived", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        LoggerService.Instance.LogInformation("You've arrived at your destination!");
                    }
                    catch (Exception ex)
                    {
                        LoggerService.Instance.LogError($"Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: {ex.Message}");
                    }
                });
            });
        }

        private void setMessagingCenterLocationErrorMessageSubscription()
        {
            MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LoggerService.Instance.LogWarning("There was an error updating location!");
                });
            });
        }

        private void setMessagingCenterStopServiceMessageSubscription()
        {
            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LoggerService.Instance.LogDebug("Location Service has been stopped!");
                });
            });
        }

        private void setMessagingCenterLocationSubscription()
        {
            MessagingCenter.Subscribe<Location>(this, "Location", location =>
            {
                LoggerService.Instance.LogDebug($"Location: latitude: {location.Latitude}, longitude: {location.Longitude}");
                
                checkIfDynamicLocationNotificationShouldBeUpdated(location);
                checkIfThereAreNotificationsThatShouldBeTriggered(location);
            });
        }
        
        private async void checkIfDynamicLocationNotificationShouldBeUpdated(Location location)
        {
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
            List<Destination> destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);

            foreach (Destination destination in destinations)
            {
                if (destination.ShouldDynamicLocationsBeUpdated(location))
                {
                    LoggerService.Instance.LogDebug($"Updating dynamic locations for {destination.Name}");
                    destination.Locations = await AzureHttpClient.Instance.GetNearbyPlaces(destination.Name, location);
                    destination.LastUpdatedLocation = location;
                    LoggerService.Instance.LogDebug($"Updated {destination.Locations.Count} dynamic locations for {destination.Name}");
                }
            }
            
            Preferences.Set(Constants.PREFERENCES_DESTINATIONS, JsonConvert.SerializeObject(destinations));
        }

        private void checkIfThereAreNotificationsThatShouldBeTriggered(Location location)
        {
            List<string> destinationsArrived = new List<string>();
            List<Notification> arrivedLocationNotifications;
            List<Notification> elapsedTimeNotification;
            
            destinationsArrived = getAllArrivedDestinations(location);
            elapsedTimeNotification = getAllElapsedTimeNotifications();
            
            if (destinationsArrived.Count > 0 || elapsedTimeNotification.Count > 0)
            {
                lock (m_NotificationsLock)
                {
                    arrivedLocationNotifications = getAllArrivedLocationNotifications(destinationsArrived);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            announceNotification(arrivedLocationNotifications, "You've arrived at your destination!");
                            announceNotification(elapsedTimeNotification, "Time elapsed for a notification!");
                            updateStatusOfSentNotifications(arrivedLocationNotifications, elapsedTimeNotification);
                        }
                        catch (Exception ex)
                        {
                            LoggerService.Instance.LogError($"Failed in checkIfThereAreNotificationsThatShouldBeTriggered: {ex.Message}");
                        }
                    });
                }
            }
        }

        private static void updateStatusOfSentNotifications(params List<Notification>[] sentNotificationLists)
        {
            string json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(json);

            notifications.ForEach(notification =>
            {
                foreach (List<Notification> sentNotificationsList in sentNotificationLists)
                {
                    if (sentNotificationsList.Any(sentNotification => sentNotification.ID.Equals(notification.ID)))
                    {
                        notification.Status = Constants.NOTIFICATION_STATUS_EXPIRED;
                        LoggerService.Instance.LogDebug($"Updated status of notification {notification.ID} to {Constants.NOTIFICATION_STATUS_EXPIRED}");
                    }
                }
            });

            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            foreach (List<Notification> sentNotificationsList in sentNotificationLists)
            {
                AzureHttpClient.Instance.UpdateNotificationsStatus(sentNotificationsList, Constants.NOTIFICATION_STATUS_EXPIRED);
            }
        }

        private List<string> getAllArrivedDestinations(Location location)
        {
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
            List<Destination> destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
            List<string> destinationsArrived = new List<string>();

            foreach (Destination destination in destinations)
            {
                if (destination.IsArrived(location))
                {
                    destinationsArrived.Add(destination.Name);
                    LoggerService.Instance.LogDebug($"Added {destination.Name} to destinations arrived list");
                }
            }

            LoggerService.Instance.LogDebug($"Arrived to {destinationsArrived.Count} destinations of out {destinations.Count}:");
            LoggerService.Instance.LogDebug($"- {string.Join($"{Environment.NewLine}- ", destinationsArrived)}");
            
            return destinationsArrived;
        }

        private List<Notification> getAllElapsedTimeNotifications()
        {
            string notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            List<Notification> elapsedTimeNotifications;
            
            LoggerService.Instance.LogInformation("Getting all elapsed time notifications");
            
            elapsedTimeNotifications = notifications
                .FindAll(
                    notification =>
                    {
                        bool isTimeNotification = notification.Type is NotificationType.Time;
                        bool isTimeElapsed = false;
                        
                        if (notification.TypeInfo is DateTime notificationTime)
                        {
                            isTimeElapsed = notificationTime <= DateTime.Now;
                        }
                        
                        bool isNewNotification = notification.Status.ToLower().Equals("new");

                        if (isTimeNotification && isTimeElapsed && isNewNotification)
                            LoggerService.Instance.LogInformation($"Found a notification {notification.ID} that it's time elapsed");
                        
                        return isTimeNotification && isTimeElapsed && isNewNotification;
                    });

            LoggerService.Instance.LogInformation($"Found a total of {elapsedTimeNotifications.Count} elapsed time notifications");
            updateAndSaveNotificationsInPrefrences(notifications, elapsedTimeNotifications);
            
            return elapsedTimeNotifications;
        }
        
        private List<Notification> getAllArrivedLocationNotifications(List<string> destinationsArrived)
        {
            string notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            List<Notification> arrivedLocationNotifications;

            arrivedLocationNotifications = notifications
                .FindAll(
                    notification =>
                    {
                        bool isRelevantType = notification.Type is NotificationType.Location || notification.Type is NotificationType.Dynamic;
                        bool isArrivedLocationNotification = destinationsArrived.Contains(notification.TypeInfo.ToString());
                        bool isNewNotification = notification.Status.ToLower().Equals("new");

                        if (isRelevantType && isArrivedLocationNotification && isNewNotification)
                            LoggerService.Instance.LogDebug($"Found arrived location notification: {notification.ID}");
                        
                        return isRelevantType && isArrivedLocationNotification && isNewNotification;
                    });

            LoggerService.Instance.LogDebug($"Found {arrivedLocationNotifications.Count} arrived location notifications");
            
            return arrivedLocationNotifications;
        }
        
        private void updateAndSaveNotificationsInPrefrences(List<Notification> allNotifications, List<Notification> toUpdateNotifications)
        {
            allNotifications.ForEach(notification =>
            {
                if (toUpdateNotifications.Contains(notification))
                {
                    notification.Status = Constants.NOTIFICATION_STATUS_SENDING;
                    LoggerService.Instance.LogInformation($"Updated status of notification {notification.ID} to '{Constants.NOTIFICATION_STATUS_SENDING}'");
                }
            });
            
            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(allNotifications));
        }
        
        private void announceNotification(List<Notification> notifications, string logMessage)
        {
            notifications.ForEach(notification =>
            {
                notificationManager.SendNotification(
                    title: notification.Name,
                    message: $"{notification.Description}{Environment.NewLine}- {notification.Creator}");

                LoggerService.Instance.LogDebug(logMessage);
                LoggerService.Instance.LogDebug($"Notification: {notification.Name}, {notification.Description}, {notification.Creator}");
            });
        }
        
        private void setNoficicationManagerNotificationReceived()
        {
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                NotificationEventArgs eventData = (NotificationEventArgs)eventArgs;

                showNotification(eventData.Title, eventData.Message);
            };
        }
        
        private void startService()
        {
            StartServiceMessage startServiceMessage = new StartServiceMessage();

            try
            {
                if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
                {
                    MessagingCenter.Send(startServiceMessage, "ServiceStarted");
                    Preferences.Set(Constants.START_LOCATION_SERVICE, false);

                    LoggerService.Instance.LogDebug("Location Service has been started!");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Instance.LogError($"Error in start service: {ex.Message}");
            }
        }

        private void showNotification(string title, string message)
        {
            LoggerService.Instance.LogInformation($"title: {title}, message: {message}");
        }
    }
}
