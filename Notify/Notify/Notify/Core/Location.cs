namespace Notify.Core
{
    public class Location
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public string Address { get; set; }
        public string Name { get; set; }

        public Location(double longitude, double latitude, string name = null, string address = null)
        {
            Longitude = longitude;
            Latitude = latitude;
            Name = name;
            Address = address;
        }

        public override string ToString()
        {
            return $"Longitude: {Longitude}, Latitude: {Latitude}";
        }
    }
}
