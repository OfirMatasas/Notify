using System.Diagnostics;
using Geolocation;
using Notify.Helpers;

namespace Notify.Core
{
    public class Destination
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private string m_Name;
        private Location m_Location;
        private string m_SSID;
        private string m_Bluetooth;

        public Destination(string name)
        {
            Name = name;
        }

        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        public Location Location
        {
            get => m_Location;
            set => m_Location = value;
        }

        public string SSID
        {
            get => m_SSID;
            set => m_SSID = value;
        }

        public string Bluetooth
        {
            get => m_Bluetooth;
            set => m_Bluetooth = value;
        }

        public bool IsArrived(Location location)
        {
            Coordinate currentLocation = new Coordinate(
                latitude: location.Latitude, 
                longitude: location.Longitude);
            Coordinate destination = new Coordinate(
                latitude: Location.Latitude, 
                longitude: Location.Longitude);
            double distance = GeoCalculator.GetDistance(
                originCoordinate: currentLocation,
                destinationCoordinate: destination, 
                distanceUnit: DistanceUnit.Meters);

            r_Logger.LogDebug($"Distance to {Name} is {distance} meters");
            return distance <= Constants.DESTINATION_MAXMIMUM_DISTANCE;
        }
    }
}
