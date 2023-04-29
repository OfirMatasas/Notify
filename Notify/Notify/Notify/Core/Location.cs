namespace Notify.Core
{
    public class Location
    {
        public double Longitude { get; }
        public double Latitude { get; }

        public Location(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        
        public override string ToString()
        {
            return $"Longitude: {Longitude}, Latitude: {Latitude}";
        }
    }
}
