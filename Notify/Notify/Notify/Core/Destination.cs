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
        private Location m_LastUpdatedLocation = new Location(0, 0);
        private bool m_IsDynamic;

        public Destination(string name, bool isDynamic = false)
        {
            Name = name;
            IsDynamic = isDynamic;
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
        
        public Location LastUpdatedLocation
        {
            get => m_LastUpdatedLocation;
            set => m_LastUpdatedLocation = value;
        }
        
        public bool IsDynamic
        {
            get => m_IsDynamic;
            set => m_IsDynamic = value;
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

        public bool ShouldLocationsBeUpdated(Location location)
        {
            Coordinate currentCoordinate, lastUpdatedLocationCoordinate;
            double distance;
            
            currentCoordinate = new Coordinate(
                latitude: location.Latitude, 
                longitude: location.Longitude);
            lastUpdatedLocationCoordinate = new Coordinate(
                latitude: LastUpdatedLocation.Latitude, 
                longitude: LastUpdatedLocation.Longitude);
            
            distance = GeoCalculator.GetDistance(
                originCoordinate: currentCoordinate,
                destinationCoordinate: lastUpdatedLocationCoordinate, 
                distanceUnit: DistanceUnit.Meters);        
            
            return distance >= Constants.DYANMIC_DESTINATION_UPDATE_DISTANCE_THRESHOLD;
        }
    }
}
