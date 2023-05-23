using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Interfaces.Managers;
using Notify.Notifications;
using Notify.Views;
using Notify.WiFi;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DriverDetailsPage = Notify.Views.DriverDetailsPage;
using Location = Notify.Core.Location;
using ProfilePage = Notify.Views.ProfilePage;
using TeamDetailsPage = Notify.Views.TeamDetailsPage;

namespace Notify
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell
    {
        private readonly LoggerService r_logger = LoggerService.Instance;
        private readonly INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        private readonly IWiFiManager m_WiFiManager = DependencyService.Get<IWiFiManager>();
        private readonly IBluetoothManager m_BluetoothManager = DependencyService.Get<IBluetoothManager>();
        private static readonly object m_NotificationsLock = new object();
        private static readonly object m_InitializeLock = new object();
        private static bool m_IsInitialized = false;
           
        public AppShell()
        {
            InitializeComponent();
            InitializeAppShell();
        }

        private void InitializeAppShell()
        {
            lock (m_InitializeLock)
            {
                if (!m_IsInitialized)
                {
                    m_IsInitialized = true;
                    Connectivity.ConnectivityChanged += internetConnectivityChanged;
                    setNoficicationManagerNotificationReceived();
                    setMessagingCenterSubscriptions();

                    if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
                    {
                        startService();
                    }

                    getBluetoothDevices();
                    retriveDestinations();
                }
            }
        }

        private void getBluetoothDevices()
        {
            m_BluetoothManager.PrintAllBondedBluetoothDevices();
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
                        r_logger.LogDebug("You've arrived at your destination!");
                    }
                    catch (Exception ex)
                    {
                        r_logger.LogError($"Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: {ex.Message}");
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
                    try
                    {
                        r_logger.LogDebug("There was an error updating location!");
                    }
                    catch (Exception ex)
                    {
                        r_logger.LogError($"Failed in MessagingCenter.Subscribe<LocationErrorMessage>: {ex.Message}");
                    }
                });
            });
        }

        private void setMessagingCenterStopServiceMessageSubscription()
        {
            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        r_logger.LogDebug("Location Service has been stopped!");

                    }
                    catch (Exception ex)
                    {
                        r_logger.LogError($"Failed in MessagingCenter.Subscribe<StopServiceMessage>: {ex.Message}");
                    }
                });
            });
        }

        private void setMessagingCenterLocationSubscription()
        {
            MessagingCenter.Subscribe<Location>(this, "Location", location =>
            {
                List<string> destinationsArrived = new List<string>();
                List<Notification> arrivedLocationNotifications;
                
                r_logger.LogDebug($"Location: latitude: {location.Latitude}, longitude: {location.Longitude}");
                
                destinationsArrived = getAllArrivedDestinations(location);

                if (destinationsArrived.Count > 0)
                {
                    lock (m_NotificationsLock)
                    {
                        arrivedLocationNotifications = getAllArrivedLocationNotifications(destinationsArrived);
                    
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                AnnounceDestinationArrival(arrivedLocationNotifications);
                                updateStatusOfSentNotifications(arrivedLocationNotifications);
                            }
                            catch (Exception ex)
                            {
                                r_logger.LogError($"Failed in MessagingCenter.Subscribe<Location>: {ex.Message}");
                            }
                        });
                    }
                }
            });
        }

        private static void updateStatusOfSentNotifications(List<Notification> arrivedLocationNotifications, LoggerService i_Logger)
        {
            string json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(json);
            
            notifications.ForEach(notification =>
            {
                if (arrivedLocationNotifications.Any(arrivedNotification => arrivedNotification.ID == notification.ID))
                {
                    notification.Status = "Sent";
                    i_Logger.LogDebug($"Updated status of notification {notification.ID} to 'Sent'");
                }
            });
            
            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            AzureHttpClient.Instance.UpdateNotificationsStatus(arrivedLocationNotifications, "Sent");
        }

        private List<string> getAllArrivedDestinations(Location location)
        {
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
            List<Destination> destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
            List<string> destinationsArrived = new List<string>();
                
            destinations.ForEach(destination =>
            {
                if (destination.IsArrived(location))
                {
                    destinationsArrived.Add(destination.Name);
                    r_logger.LogDebug($"Added {destination.Name} to destinations arrived list");
                }
            });
            
            r_logger.LogDebug($"Arrived to {destinationsArrived.Count} destinations of out {destinations.Count}:");
            r_logger.LogDebug($"- {string.Join($"{Environment.NewLine}- ", destinationsArrived)}");
            
            return destinationsArrived;
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
                        bool isLocationNotification = notification.Type is NotificationType.Location;
                        bool isArrivedLocationNotification = destinationsArrived.Contains(notification.TypeInfo.ToString());
                        bool isNewNotification = notification.Status.ToLower().Equals("new");

                        if (isLocationNotification && isArrivedLocationNotification && isNewNotification)
                            r_logger.LogDebug($"Found arrived location notification: {notification.ID}");
                        
                        return isLocationNotification && isArrivedLocationNotification && isNewNotification;
                    });

            r_logger.LogDebug($"Found {arrivedLocationNotifications.Count} arrived location notifications");

            
            notifications.ForEach(notification =>
            {
                if (arrivedLocationNotifications.Contains(notification))
                {
                    notification.Status = "Sending";
                    r_logger.LogDebug($"Updated status of notification {notification.ID} to 'Sending'");
                }
            });
            
            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            r_logger.LogDebug($"Found {arrivedLocationNotifications.Count} arrived location notifications");
            return arrivedLocationNotifications;
        }

        private void AnnounceDestinationArrival(List<Notification> arrivedLocationNotifications)
        {
            arrivedLocationNotifications.ForEach(notification =>
            {
                notificationManager.SendNotification(
                    title: notification.Name,
                    message: $"{notification.Description}{Environment.NewLine}- {notification.Creator}");
                
                r_logger.LogDebug($"You've arrived at your {notification.TypeInfo} destination!");
                r_logger.LogDebug($"Notification: {notification.Name}, {notification.Description}, {notification.Creator}");
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

                    r_logger.LogDebug("Location Service has been started!");
                }
            }
            catch (Exception ex)
            {
                r_logger.LogError(ex.Message);
            }
        }

        private void showNotification(string title, string message)
        {
            r_logger.LogDebug($"title: {title}, message: {message}");
        }
    }
}
