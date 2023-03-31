using System;
using System.Diagnostics;
using Notify.Helpers;
using Notify.HttpClient;
using Notify.Notifications;
using Notify.Views;
using Plugin.Geolocator.Abstractions;
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
    public partial class AppShell : Shell
    {
        private readonly INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        private Location m_LastUpdatedLocation = null;
        
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            
            setNoficicationManagerNotificationReceived();
            setMessagingCenterSubscriptions();

            if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
            {
                StartService();
            }
        }

        void RegisterRoutes()
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
        
        private void setMessagingCenterLocationArrivedMessageSubscription()
        {
            MessagingCenter.Subscribe<LocationArrivedMessage>(this, "LocationArrived", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Debug.Write("You've arrived at your destination!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Write($"Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: {ex.Message}");
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
                        Debug.Write("There was an error updating location!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Write($"Failed in MessagingCenter.Subscribe<LocationErrorMessage>: {ex.Message}");
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
                        Debug.Write("Location Service has been stopped!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Write($"Failed in MessagingCenter.Subscribe<StopServiceMessage>: {ex.Message}");
                    }
                });
            });
        }

        private void setMessagingCenterLocationSubscription()
        {
            MessagingCenter.Subscribe<Location>(this, "Location", location =>
            {
                bool arrived;

                if (requiresLocationUpdate(location))
                {
                    Debug.WriteLine($"{location}, {DateTime.Now.ToLongTimeString()}");
                    arrived = AzureHttpClient.Instance.CheckIfArrivedDestination(location);
                    
                    if (arrived)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                AnnounceDestinationArrival();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Failed in MessagingCenter.Subscribe<Location>: {ex.Message}");
                            }
                        });
                    }
                    
                    m_LastUpdatedLocation = location;
                    Debug.WriteLine($"Updated last updated location: {m_LastUpdatedLocation}");
                }
            });
        }
        
        bool requiresLocationUpdate(Location location)
        {
            bool shouldUpdate;

            if (m_LastUpdatedLocation == null)
            {
                shouldUpdate = true;
            }
            else
            {
                double distance = GeolocatorUtils.CalculateDistance(
                    latitudeStart: location.Latitude, longitudeStart: location.Longitude,
                    latitudeEnd: location.Latitude, longitudeEnd: m_LastUpdatedLocation.Longitude,
                    units: GeolocatorUtils.DistanceUnits.Kilometers) * Constants.METERS_IN_KM;
            
                Debug.WriteLine($"Distance from last updated location: {distance} meters");
                shouldUpdate = distance > Constants.DISTANCE_UPDATE_THRESHOLD;
            }

            return shouldUpdate;
        }

        private void AnnounceDestinationArrival()
        {
            notificationManager.SendNotification("Destination arrived!", "You've arrived at your destination");
            Debug.WriteLine("You've arrived at your destination!");
        }

        private void setNoficicationManagerNotificationReceived()
        {
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                NotificationEventArgs eventData = (NotificationEventArgs)eventArgs;

                showNotification(eventData.Title, eventData.Message);
            };
        }
        
        private void StartService()
        {
            StartServiceMessage startServiceMessage = new StartServiceMessage();

            try
            {
                if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
                {
                    MessagingCenter.Send(startServiceMessage, "ServiceStarted");
                    Preferences.Set(Constants.START_LOCATION_SERVICE, false);

                    Debug.WriteLine("Location Service has been started!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void showNotification(string title, string message)
        {
            Debug.WriteLine($"title: {title}, message: {message}");
        }
    }
}
