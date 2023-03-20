using System;
using System.Diagnostics;
using Notify.Services.Location;
using Notify.Views;
using Plugin.FirebasePushNotification;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.CommunityToolkit.UI.Views;
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
                        Debug.WriteLine("You've arrived your destination!");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: " + ex.Message);
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
                        Debug.WriteLine("There was an error updating location!");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed in MessagingCenter.Subscribe<LocationErrorMessage>: " + ex.Message);
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
                        Debug.WriteLine("Location Service has been stopped!");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed in MessagingCenter.Subscribe<StopServiceMessage>: " + ex.Message);
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
                        Debug.WriteLine($"{Environment.NewLine}{location.Latitude}, {location.Longitude}, {DateTime.Now.ToLongTimeString()}");

                        if (checkIfArrivedDestinationForTheFirstTime(location))
                        {
                            arrivedDestinationForTheFirstTime();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Failed in MessagingCenter.Subscribe<LocationMessage>: " + ex.Message);
                    }
                });
            });
        }

        private void arrivedDestinationForTheFirstTime()
        {
            Debug.WriteLine("You've arrived your desitnation!");
            notificationManager.SendNotification("Destination arrived!", "You're arrived your destionation");

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

        private async void Button_Clicked(Object sender, EventArgs e)
        {
            PermissionStatus permission = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (permission.Equals(PermissionStatus.Denied))
            {
                // TODO Let the user know they need to accept
                Console.WriteLine("Permission denied.");
                return;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                buttonClickedOnIOS();
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                buttonClickedOnAndroid();
            }
        }

        private void buttonClickedOnAndroid()
        {
            if (Preferences.Get("LocationServiceRunning", false) == false)
            {
                StartService();
            }
            else
            {
                StopService();
            }
        }

        private async void buttonClickedOnIOS()
        {
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
                CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;

                return;
            }

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, false, new ListenerSettings
            {
                ActivityType = ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = false,
                DeferralDistanceMeters = 10,
                DeferralTime = TimeSpan.FromSeconds(5),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = true
            });

            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
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
                Console.WriteLine(e.Message);
            }
        }

        private void StopService()
        {
            StopServiceMessage stopServiceMessage = new StopServiceMessage();

            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }

        private void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            LocationMessage location = new LocationMessage
            {
                Latitude = e.Position.Latitude,
                Longitude = e.Position.Longitude
            };

            Debug.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}");

            if (checkIfArrivedDestinationForTheFirstTime(location))
            {
                notificationManager.SendNotification("Destination arrived!", "You're arrived your destionation");
                Debug.WriteLine("You've arrived your destination!");
            }
        }

        private void showNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine($"Notification Received:\nTitle: {title}\nMessage: {message}");
            });
        }
    }
}
