using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Notify.Helpers;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = Notify.Core.Location;

namespace Notify
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
                        location = await Geolocation.GetLocationAsync(request, token);
                        
                        if (location != null)
                        {
                            Location message = new Location(longitude: location.Longitude, latitude: location.Latitude);

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
                if (CrossGeolocator.Current.IsListening)
                {
                    await CrossGeolocator.Current.StopListeningAsync();

                    return;
                }

                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, false,
                    new Plugin.Geolocator.Abstractions.ListenerSettings
                    {
                        ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                        AllowBackgroundUpdates = true,
                        DeferLocationUpdates = false,
                        DeferralDistanceMeters = 10,
                        DeferralTime = TimeSpan.FromSeconds(5),
                        ListenForSignificantChanges = true,
                        PauseLocationUpdatesAutomatically = true
                    });
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                if (Preferences.Get(Constants.START_LOCATION_SERVICE, false) == false)
                {
                    StartService();
                }
                else
                {
                    StopService();
                }
            }
        }
        
        public void SubscribeToLocationMessaging()
        {
            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                MessagingCenter.Subscribe<Location>(this, "Location",
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
                    StartService();
                }
            }
        }

        public void StartService()
        {
            StartServiceMessage message = new StartServiceMessage();
            MessagingCenter.Send(message, "Location service started!");
            Preferences.Set(Constants.START_LOCATION_SERVICE, true);
            Debug.WriteLine("Location service as been started!");
        }
        
        public void StopService()
        {
            StopServiceMessage message = new StopServiceMessage();
            MessagingCenter.Send(message, "Location service stopped!");
            Preferences.Set(Constants.START_LOCATION_SERVICE, false);
            Debug.WriteLine("Location service as been stopped!");
        }
    }
}
