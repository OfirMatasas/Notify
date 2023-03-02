using GeoCoordinatePortable;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Notify.WebApplication;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

GeoCoordinate destination = GeoCoordinate.Unknown;

app.MapPost(pattern: "/location/destination", handler: ([FromBody] Location location) =>
{
    destination = new GeoCoordinate
    {
        Latitude = location.Latitude, 
        Longitude = location.Longitude
    };
});

app.MapPost(pattern: "/location/distance", handler: ([FromBody] Location location) =>
{
    GeoCoordinate src = new GeoCoordinate
    {
        Latitude = location.Latitude, 
        Longitude = location.Longitude
    };

    if (destination.Equals(GeoCoordinate.Unknown))
    {
        return -1;
    }
    
    return destination.GetDistanceTo(src);
});

app.Run();
