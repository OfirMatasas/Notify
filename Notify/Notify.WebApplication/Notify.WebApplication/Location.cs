namespace Notify.WebApplication;

public class Location
{
    public double Longitude { get; }
    public double Latitude { get; }

    public Location(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
}
