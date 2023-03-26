using System;
using System.Diagnostics;
using Notify.Notifications;
using Notify.Views;
using Plugin.Geolocator.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DriverDetailsPage = Notify.Views.DriverDetailsPage;
using ProfilePage = Notify.Views.ProfilePage;
using TeamDetailsPage = Notify.Views.TeamDetailsPage;

namespace Notify
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        readonly INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        readonly Location goalLocation = new Location(latitude: 32.02069, longitude: 34.763419999999996);
        bool arrivedDestination = false;
        
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            
            setNoficicationManagerNotificationReceived();
            setMessagingCenterSubscriptions();

            if (Preferences.Get("LocationServiceRunning", false) == true)
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
            setMessagingCenterLocationMessageSubscription();
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
                        Debug.Write("You've arrived your destination!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Write("Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: " + ex.Message);
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
                        Debug.Write("Failed in MessagingCenter.Subscribe<LocationErrorMessage>: " + ex.Message);
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
                        Debug.Write("Failed in MessagingCenter.Subscribe<StopServiceMessage>: " + ex.Message);
                    }
                });
            });
        }

        private void setMessagingCenterLocationMessageSubscription()
        {
            MessagingCenter.Subscribe<LocationMessage>(this, "Location", location =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Debug.WriteLine($"{location.Latitude}, {location.Longitude}, {DateTime.Now.ToLongTimeString()}");

                        if (checkIfArrivedDestinationForTheFirstTime(location))
                        {
                            arrivedDestinationForTheFirstTime();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed in MessagingCenter.Subscribe<LocationMessage>: " + ex.Message);
                    }
                });
            });
        }
        
        private void arrivedDestinationForTheFirstTime()
        {
            notificationManager.SendNotification("Destination arrived!", "You're arrived your destination");
            Debug.WriteLine("You've arrived your destination!");

            arrivedDestination = true;
        }
        
        private bool checkIfArrivedDestinationForTheFirstTime(LocationMessage location)
        {
            return !arrivedDestination && location.Latitude == goalLocation.Latitude && location.Longitude == goalLocation.Longitude;
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
                MessagingCenter.Send(startServiceMessage, "ServiceStarted");
                Preferences.Set("LocationServiceRunning", true);

                Debug.WriteLine("Location Service has been started!");
                Debug.WriteLine($"Goal destination: {goalLocation.Latitude},{goalLocation.Longitude}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void showNotification(string title, string message)
        {
            Debug.WriteLine($"title: {title}, message: {message}");
        }
    }
}