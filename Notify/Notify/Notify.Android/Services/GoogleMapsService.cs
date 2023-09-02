using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Geolocation;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Services;
using Xamarin.Essentials;
using Location = Xamarin.Essentials.Location;
using Uri = Android.Net.Uri;

namespace Notify.Droid.Services
{
    public class GoogleMapsService : ExternalMapsService
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private readonly Context r_Context;
        private static readonly object r_LockInstanceCreation = new object();
        private static GoogleMapsService m_Instance;

        private GoogleMapsService(Context context)
        {
            r_Context = context;
        }

        public static ExternalMapsService Initialize(Context context)
        {
            if (m_Instance == null)
            {
                lock (r_LockInstanceCreation)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = new GoogleMapsService(context);
                    }
                }
            }
            return m_Instance;
        }
        
        public static GoogleMapsService GetInstance()
        {
            if (m_Instance == null)
            {
                throw new InvalidOperationException("MyMapHandler has not been initialized. Call Initialize first.");
            }
            
            return (GoogleMapsService)m_Instance;
        }

        public void OpenGoogleMapsNavigation(double latitude, double longitude)
        {
            Uri uri;
            Intent intent;
            //if (IsGoogleMapsInstalled()) - TODO  - uncomment this line and check on physical device

            uri = Uri.Parse($"google.navigation:q={latitude},{longitude}");
            intent = new Intent(Intent.ActionView, uri);
            intent.SetPackage("com.google.android.apps.maps");
            r_Context.StartActivity(intent);
        }

        private bool IsGoogleMapsInstalled()
        {
            bool isSucceeded;
            Uri uri;
            Intent intent;
            
            try
            {
                uri = Uri.Parse("com.google.android.apps.maps");
                intent = new Intent(Intent.ActionView, uri);
                intent.SetPackage("com.google.android.apps.maps");
                isSucceeded = r_Context.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly)
                    .Count > 0;
            }
            catch (Exception ex)
            {
                r_Logger.LogWarning($"Something went wrong.{Environment.NewLine}{ex.Message}");
                isSucceeded = false;
            }

            return isSucceeded;
        }

        private static async Task<Core.Location> GetNearestPlace(string placeType, Core.Location currentLocation)
        {
            Core.Location nearestPlace = null;
            List<Core.Location> nearbyPlaces =
                await AzureHttpClient.Instance.GetNearbyPlaces(placeType, currentLocation);
            Coordinate currentCoordinate, placeCoordinate;
            double distance, minDistance;

            currentCoordinate = new Coordinate(
                latitude: currentLocation.Latitude,
                longitude: currentLocation.Longitude);

            if (nearbyPlaces.Count > 0)
            {
                nearestPlace = nearbyPlaces[0];
                placeCoordinate = new Coordinate(
                    latitude: nearestPlace.Latitude,
                    longitude: nearestPlace.Longitude);

                minDistance = GeoCalculator.GetDistance(
                    originCoordinate: currentCoordinate,
                    destinationCoordinate: placeCoordinate,
                    distanceUnit: DistanceUnit.Meters);

                for (int i = 1; i < nearbyPlaces.Count; i++)
                {
                    placeCoordinate.Latitude = nearbyPlaces[i].Latitude;
                    placeCoordinate.Longitude = nearbyPlaces[i].Longitude;

                    distance = GeoCalculator.GetDistance(
                        originCoordinate: currentCoordinate,
                        destinationCoordinate: placeCoordinate,
                        distanceUnit: DistanceUnit.Meters);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPlace = nearbyPlaces[i];
                    }
                }
            }

            return nearestPlace;
        }

        public override async void OpenExternalMap(string notificationType)
         {
             double nearestPlaceLatitude, nearestPlaceLongitude;
             Core.Location currentLocation;
             Core.Location nearestPlace;
             GeolocationRequest request;
             Location location;

             try
             {
                 request = new GeolocationRequest(GeolocationAccuracy.High);
                 location = await Xamarin.Essentials.Geolocation.GetLocationAsync(request);

                 currentLocation = new Core.Location(location.Longitude, location.Latitude);
                 nearestPlace = await GetNearestPlace(notificationType, currentLocation);
                
                 if (nearestPlace != null)
                 {
                     nearestPlaceLatitude = nearestPlace.Latitude;
                     nearestPlaceLongitude = nearestPlace.Longitude;
                     GetInstance().OpenGoogleMapsNavigation(nearestPlaceLatitude, nearestPlaceLongitude);
                 }
                 else
                 {
                     await App.Current.MainPage.DisplayAlert("", $"No {notificationType} nearby.", "OK");
                 }
             }
             catch (Exception ex)
             {
                 r_Logger.LogError($"OnOpenGoogleMapsAppButtonClicked: {ex.Message}");
             }
         }
    }
}