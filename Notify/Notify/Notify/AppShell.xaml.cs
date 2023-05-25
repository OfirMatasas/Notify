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
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private readonly INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        private readonly IWiFiManager m_WiFiManager = DependencyService.Get<IWiFiManager>();
        private readonly IBluetoothManager m_BluetoothManager = DependencyService.Get<IBluetoothManager>();
           
        public AppShell()
        {
            InitializeComponent();
            registerRoutes();
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

        private void getBluetoothDevices()
        {
            m_BluetoothManager.PrintAllBondedBluetoothDevices();
        }

        private void internetConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            m_WiFiManager.PrintConnectedWiFi(sender, e);
            m_WiFiManager.SendNotifications(sender, e);
        }

        private void registerRoutes()
        {
            Routing.RegisterRoute("profile", typeof(ProfilePage));
            Routing.RegisterRoute("schedule/details", typeof(CircuitDetailsPage));
            Routing.RegisterRoute("schedule/details/laps", typeof(CircuitLapsPage));
            Routing.RegisterRoute("drivers/details", typeof(DriverDetailsPage));
            Routing.RegisterRoute("teams/details", typeof(TeamDetailsPage));
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
                        r_Logger.LogInformation("You've arrived at your destination!");
                    }
                    catch (Exception ex)
                    {
                        r_Logger.LogError($"Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: {ex.Message}");
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
                        r_Logger.LogWarning("There was an error updating location!");
                    }
                    catch (Exception ex)
                    {
                        r_Logger.LogError($"Failed in MessagingCenter.Subscribe<LocationErrorMessage>: {ex.Message}");
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
                        r_Logger.LogDebug("Location Service has been stopped!");

                    }
                    catch (Exception ex)
                    {
                        r_Logger.LogError($"Failed in MessagingCenter.Subscribe<StopServiceMessage>: {ex.Message}");
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
                
                destinationsArrived = getAllArrivedDestinations(location);

                if (destinationsArrived.Count > 0)
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
                            r_Logger.LogError($"Failed in MessagingCenter.Subscribe<Location>: {ex.Message}");
                        }
                    });
                }
            });
        }

        private static void updateStatusOfSentNotifications(List<Notification> arrivedLocationNotifications)
        {
            string json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
            List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(json);
            
            notifications.ForEach(notification =>
            {
                if (arrivedLocationNotifications.Any(arrivedNotification => arrivedNotification.ID == notification.ID))
                {
                    notification.Status = "Sent";
                    r_Logger.LogDebug($"Updated status of notification {notification.ID} to 'Sent'");
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
                    r_Logger.LogDebug($"Added {destination.Name} to destinations arrived list");
                }
            });
            
            r_Logger.LogDebug($"Arrived to {destinationsArrived.Count} destinations of out {destinations.Count}:");
            r_Logger.LogDebug($"- {string.Join($"{Environment.NewLine}- ", destinationsArrived)}");
            
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
                            r_Logger.LogDebug($"Found arrived location notification: {notification.Name}");
                        
                        return isLocationNotification && isArrivedLocationNotification && isNewNotification;
                    });

            r_Logger.LogDebug($"Found {arrivedLocationNotifications.Count} arrived location notifications");

            return arrivedLocationNotifications;
        }

        private void AnnounceDestinationArrival(List<Notification> arrivedLocationNotifications)
        {
            arrivedLocationNotifications.ForEach(notification =>
            {
                notificationManager.SendNotification(
                    title: notification.Name,
                    message: $"{notification.Description}{Environment.NewLine}- {notification.Creator}");
                
                r_Logger.LogDebug($"You've arrived at your {notification.TypeInfo} destination!");
                r_Logger.LogDebug($"Notification: {notification.Name}, {notification.Description}, {notification.Creator}");
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

                    r_Logger.LogDebug("Location Service has been started!");
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError(ex.Message);
            }
        }

        private void showNotification(string title, string message)
        {
            r_Logger.LogInformation($"title: {title}, message: {message}");
        }
    }
}
