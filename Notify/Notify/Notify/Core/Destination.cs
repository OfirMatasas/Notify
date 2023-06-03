using System.Collections.Generic;
using Geolocation;
using Notify.Helpers;

namespace Notify.Core
{
    public class Destination
    {
        private string m_Name;
        private List<Location> m_Locations = new List<Location>();
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

        public List<Location> Locations
        {
            get => m_Locations;
            set => m_Locations = value;
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

        public bool IsArrived(Location currentLocation)
        {
            Coordinate currentCoordinate, destinationCoordinate;
            double distance;
            bool isArrived = false;

            foreach (Location location in Locations)
            {
                currentCoordinate = new Coordinate(
                    latitude: location.Latitude, 
                    longitude: location.Longitude);
                destinationCoordinate = new Coordinate(
                    latitude: currentLocation.Latitude, 
                    longitude: currentLocation.Longitude);
                
                distance = GeoCalculator.GetDistance(
                    originCoordinate: currentCoordinate,
                    destinationCoordinate: destinationCoordinate, 
                    distanceUnit: DistanceUnit.Meters);
                
                if(distance <= Constants.DESTINATION_MAXMIMUM_DISTANCE)
                {
                    LoggerService.Instance.LogDebug($"Destination {Name} is arrived.");
                    isArrived = true;
                    break;
                }
            }

            return isArrived;
        }
    }
}
