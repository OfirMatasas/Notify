using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Notify.Helpers;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.Services.Location
{
    public class LocationService
    {
        private bool m_Stopping = false;

        public async Task Run(CancellationToken token)
        {
            await Task.Run(async () => {
                GeolocationRequest request;
                Xamarin.Essentials.Location location;

                while (!m_Stopping)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        await Task.Delay(2000);

                        request = new GeolocationRequest(GeolocationAccuracy.High);
                        location = await Xamarin.Essentials.Geolocation.GetLocationAsync(request, token);
                        
                        if (location != null)
                        {
                            Core.Location message = new Core.Location(longitude: location.Longitude, latitude: location.Latitude);

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                MessagingCenter.Send(message, "Location");
                            });
                        }
                    }
                    catch (Exception)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            LocationErrorMessage errorMessage = new LocationErrorMessage();
                            MessagingCenter.Send(errorMessage, "LocationError");
                        });
                    }
                }
            }, token);
        }

        public async Task ManageLocationTracking()
        {
            PermissionStatus permission = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (permission == PermissionStatus.Denied)
            {
                // TODO Let the user know they need to accept
                return;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(10), 10, false,
                    new Plugin.Geolocator.Abstractions.ListenerSettings
                    {
                        ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                        AllowBackgroundUpdates = true,
                        DeferLocationUpdates = true,
                        DeferralDistanceMeters = 10,
                        DeferralTime = TimeSpan.FromSeconds(5), 
                        ListenForSignificantChanges = true,
                        PauseLocationUpdatesAutomatically = true
                    });
                CrossGeolocator.Current.PositionChanged += (sender, args) =>
                {
                    MessagingCenter.Send<Core.Location>(new Core.Location(args.Position.Longitude, args.Position.Latitude), "Location");
                    Debug.WriteLine($"Current location: {args.Position.Latitude},{args.Position.Longitude}");
                };
            }
            
            if (Preferences.Get(Constants.START_LOCATION_SERVICE, false) == false)
            {
                startService();
            }
            else
            {
                stopService();
            }
        }
        
        public void SubscribeToLocationMessaging()
        {
            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                MessagingCenter.Subscribe<Core.Location>(this, "Location",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Debug.WriteLine(
                                $"User's current location: {message}, {DateTime.Now.ToLongTimeString()}");
                        });
                    });

                MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() => { Debug.WriteLine("Location Service has been stopped!"); });
                    });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() => { Debug.WriteLine("There was an error updating location!"); });
                    });

                if (Preferences.Get(Constants.START_LOCATION_SERVICE, false))
                {
                    startService();
                }
            }
        }

        private void startService()
        {
            StartServiceMessage message = new StartServiceMessage();
            MessagingCenter.Send(message, "Location service started!");
            Preferences.Set(Constants.START_LOCATION_SERVICE, true);
            Debug.WriteLine("Location service has been started!");
        }

        private void stopService()
        {
            StopServiceMessage message = new StopServiceMessage();
            MessagingCenter.Send(message, "Location service stopped!");
            Preferences.Set(Constants.START_LOCATION_SERVICE, false);
            Debug.WriteLine("Location service has been stopped!");
        }
    }
}
