using System;
using System.Threading;
using System.Threading.Tasks;
using Notify.Helpers.Messages;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.Services.Location
{
    public class GetLocationService
    {
        readonly bool stopping = false;

        public GetLocationService()
        {
        }

        public async Task Run(CancellationToken token)
        {
            await Task.Run(async () => {
                while (!stopping)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        await Task.Delay(2000);

                        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High);
                        Xamarin.Essentials.Location location = await Geolocation.GetLocationAsync(request);

                        if (location != null)
                        {
                            LocationMessage message = new LocationMessage
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
                            LocationErrorMessage errorMessage = new LocationErrorMessage();

                            MessagingCenter.Send(errorMessage, "LocationError");
                        });
                    }
                }
                return;
            }, token);
        }
    }
}