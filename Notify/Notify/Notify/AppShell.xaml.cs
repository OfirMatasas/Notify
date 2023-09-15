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
using NotificationEventArgs = Notify.Notifications.NotificationEventArgs;

namespace Notify
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private readonly INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        private readonly IWiFiManager m_WiFiManager = DependencyService.Get<IWiFiManager>();
        private static readonly object m_NotificationsLock = new object();
        private static readonly object m_InitializeLock = new object();
        private static bool m_IsInitialized;
        private BluetoothManager m_BluetoothManager;
        private DateTime m_LastTimeCheckedForNewsfeeds = DateTime.MinValue;

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
                    setNotificationManagerNotificationReceived();
                    setMessagingCenterSubscriptions();
                    
                    if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
                    {
                        startService();
                    }

                    retrieveDestinations();
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

        private async void retrieveDestinations()
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
                sendAllRelevantLocationNotifications(location);
                getNewsfeeds();
            });
        }
        
        private async void getNewsfeeds()
        {
            if (DateTime.Now - m_LastTimeCheckedForNewsfeeds > TimeSpan.FromMinutes(1))
            {
                LoggerService.Instance.LogInformation("Getting newsfeeds");
                List<Newsfeed> newsfeeds = await AzureHttpClient.Instance.GetNewsfeeds();
                LoggerService.Instance.LogInformation($"Got {newsfeeds.Count} newsfeeds");

                foreach (Newsfeed newsfeed in newsfeeds)
                {
                    LoggerService.Instance.LogDebug($"Sending newsfeed: {newsfeed.Title}, {newsfeed.Content}");
                    DependencyService.Get<INotificationManager>().SendNewsfeed(newsfeed);
                }
                
                m_LastTimeCheckedForNewsfeeds = DateTime.Now;
            }
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

        private void sendAllRelevantLocationNotifications(Location location)
        {
            string notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            List<Notification> sentNotifications = new List<Notification>();
            List<Notification> arrivedNotifications = new List<Notification>();
            List<Notification> permanentNotifications = new List<Notification>();
            
            lock (m_NotificationsLock)
            {
                getAllNotificationsForArrivalDestinations(location, ref sentNotifications, ref arrivedNotifications);
                getAllNotificationsForLeaveDestinations(location, ref sentNotifications, ref permanentNotifications);
                getAllNotificationsForElapsedTime(ref sentNotifications);
                Utils.CheckForExpiredPendingTimeNotifications(notifications);

                updateStatusOfNotifications(Constants.NOTIFICATION_STATUS_EXPIRED, sentNotifications);
                updateStatusOfNotifications(Constants.NOTIFICATION_STATUS_ARRIVED, arrivedNotifications);
                updateStatusOfNotifications(Constants.NOTIFICATION_STATUS_ACTIVE, permanentNotifications);
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

            LoggerService.Instance.LogDebug($"Arrived to {destinationsArrived.Count} destinations out of {destinations.Count}:");
            LoggerService.Instance.LogDebug($"- {string.Join($"{Environment.NewLine}- ", destinationsArrived)}");
            
            return destinationsArrived;
        }
        
        private void getAllNotificationsForArrivalDestinations(Location location, ref List<Notification> sentNotifications, ref List<Notification> arrivedNotifications)
        {
            string notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            List<string> destinationsArrived = getAllArrivedDestinations(location);
            bool isRelevantType, isArrivalNotification, isDestinationArrived, isActive;

            r_Logger.LogInformation("Sending location notifications for arrival destinations");

            foreach (Notification notification in notifications)
            {
                isRelevantType = notification.Type is NotificationType.Location || notification.Type is NotificationType.Dynamic;
                isDestinationArrived = destinationsArrived.Contains(notification.TypeInfo.ToString());
                isArrivalNotification = notification.Activation.Equals(Constants.NOTIFICATION_ACTIVATION_ARRIVAL);
                isActive = notification.Status.Equals(Constants.NOTIFICATION_STATUS_ACTIVE);

                if (isRelevantType && isActive && isDestinationArrived)
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
                            r_Logger.LogInformation($"Adding notification {notification.ID} is permanent, therefore adding it to arrived notifications list");
                            arrivedNotifications.Add(notification);
                        }
                        else
                        {
                            r_Logger.LogInformation($"Adding notification {notification.ID} is not permanent, therefore adding it to sent notifications list");
                            sentNotifications.Add(notification);
                        }
                    }
                    else
                    {
                        r_Logger.LogInformation($"Adding notification {notification.ID} is permanent, therefore adding it to arrived notifications list");
                        arrivedNotifications.Add(notification);
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending location notifications for arrival destinations");
        }
        
        private List<string> getAllLeftDestinations(Location location)
        {
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
            List<Destination> destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
            List<string> destinationsLeft = new List<string>();

            foreach (Destination destination in destinations)
            {
                if (!destination.IsDynamic && destination.IsLeft(location))
                {
                    destinationsLeft.Add(destination.Name);
                    LoggerService.Instance.LogDebug($"Added {destination.Name} to destinations left list");
                }
            }

            LoggerService.Instance.LogDebug($"Left {destinationsLeft.Count} destinations out of {destinations.Count}:");
            LoggerService.Instance.LogDebug($"- {string.Join($"{Environment.NewLine}- ", destinationsLeft)}");
            
            return destinationsLeft;
        }
        
        private void getAllNotificationsForLeaveDestinations(Location location, ref List<Notification> sentNotifications, ref List<Notification> permanentNotifications)
        {
            string notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            List<string> destinationsLeft = getAllLeftDestinations(location);
            bool isLocationType, isDestinationLeft, isArrived, isLeaveNotification;

            r_Logger.LogInformation("Sending location notifications for leave destinations");

            foreach (Notification notification in notifications)
            {
                isLocationType = notification.Type is NotificationType.Location;
                isDestinationLeft = destinationsLeft.Contains(notification.TypeInfo.ToString());
                isLeaveNotification = notification.Activation.Equals(Constants.NOTIFICATION_ACTIVATION_LEAVE);
                isArrived = notification.Status.Equals(Constants.NOTIFICATION_STATUS_ARRIVED);

                if (isLocationType && isDestinationLeft && isArrived)
                {
                    if (isLeaveNotification)
                    {
                        if (notification.ShouldBeNotified.Equals(notification.Creator))
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
                            r_Logger.LogInformation($"Adding notification {notification.ID} is permanent, therefore adding it to permanent notifications list");
                            permanentNotifications.Add(notification);
                        }
                        else
                        {
                            r_Logger.LogInformation($"Adding notification {notification.ID} is not permanent, therefore adding it to sent notifications list");
                            sentNotifications.Add(notification);
                        }
                    }
                    else if(notification.IsPermanent)
                    {
                        r_Logger.LogInformation($"Adding notification {notification.ID} is permanent, therefore adding it to permanent notifications list");
                        permanentNotifications.Add(notification);
                    }
                }
            }
            
            r_Logger.LogInformation("Finished sending location notifications for arrival destinations");
        }

        private List<Notification> getAllElapsedTimeNotifications()
        {
            string notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            List<Notification> elapsedTimeNotifications = new List<Notification>();
            bool isActive, isTimeNotification, isTimeElapsed;
            
            LoggerService.Instance.LogInformation("Getting all elapsed time notifications");
            
            foreach (Notification notification in notifications)
            {
                isTimeElapsed = false;
                isActive = notification.Status.Equals(Constants.NOTIFICATION_STATUS_ACTIVE);
                isTimeNotification = notification.Type is NotificationType.Time;
                
                if (notification.TypeInfo is DateTime notificationTime)
                {
                    isTimeElapsed = notificationTime <= DateTime.Now;
                }
                
                if (isTimeNotification && isTimeElapsed && isActive)
                {
                    elapsedTimeNotifications.Add(notification);
                    LoggerService.Instance.LogInformation($"Found a notification {notification.ID} that its time elapsed");
                }
            }
            
            LoggerService.Instance.LogInformation($"Found a total of {elapsedTimeNotifications.Count} elapsed time notifications");
            return elapsedTimeNotifications;
        }

        private void getAllNotificationsForElapsedTime(ref List<Notification> sentNotifications)
        {
            List<Notification> elapsedTimeNotification = getAllElapsedTimeNotifications();
            
            LoggerService.Instance.LogInformation("Sending elapsed time notifications");

            foreach (Notification notification in elapsedTimeNotification)
            {
                LoggerService.Instance.LogInformation($"Sending notification {notification.ID}");
                DependencyService.Get<INotificationManager>().SendNotification(notification);
                sentNotifications.Add(notification);
            }
            
            LoggerService.Instance.LogInformation("Finished sending elapsed time notifications");
        }
        
        private void updateStatusOfNotifications(string newStatus, List<Notification> sentNotifications)
        {
            string json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(json);

            foreach (Notification notification in notifications)
            {
                if (sentNotifications.Any(sentNotification => sentNotification.ID.Equals(notification.ID)))
                {
                    notification.Status = newStatus;
                    LoggerService.Instance.LogInformation($"Updated status of notification {notification.ID} to {newStatus}");
                }
            }

            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            AzureHttpClient.Instance.UpdateNotificationsStatus(sentNotifications, newStatus);
        }

        private void setNotificationManagerNotificationReceived()
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
