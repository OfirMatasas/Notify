using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify
{
    public class LocationService
    {
        readonly bool stopping = false;
        
        public async Task Run(CancellationToken token)
        {
            await Task.Run(async () => {
                while (!stopping)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        await Task.Delay(2000);

                        var request = new GeolocationRequest(GeolocationAccuracy.High);
                        var location = await Geolocation.GetLocationAsync(request, token);
                        if (location != null)
                        {
                            var message = new LocationMessage
                            {
                                Latitude = location.Latitude,
                                Longitude = location.Longitude
                            };

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
                            var errormessage = new LocationErrorMessage();
                            MessagingCenter.Send(errormessage, "LocationError");
                        });
                    }
                }
            }, token);
        }

        public void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            Debug.WriteLine($"User's current location: {e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}");
        }

        public async Task ManageLocationTracking()
        {
            var permission = await Permissions.RequestAsync<Permissions.LocationAlways>();

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
                    CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;

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

                CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            }
            else if (Device.RuntimePlatform == Device.Android)
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
        }
        
        public void SubscribeToLocationMessaging()
        {
            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                MessagingCenter.Subscribe<LocationMessage>(this, "Location",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Debug.WriteLine(
                                $"User's current location: {message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}");
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

                if (Preferences.Get("LocationServiceRunning", false) == true)
                {
                    StartService();
                }
            }
        }
        
        public void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            Preferences.Set("LocationServiceRunning", true);
            Debug.WriteLine("Location Service has been started!");
        }

        public void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }
    }
}