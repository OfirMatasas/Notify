using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Geolocation;
using Microsoft.AspNetCore.Authorization;

namespace Notify.Functions.NotifyFunctions.Location
{
    public static class Distance
    { 
        [FunctionName("Distance")]
        [AllowAnonymous]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Distance")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Distance function: Got client's current location HTTP request.");
            Coordinate currentLocation, destination;

            try
            {
                // Get the input JSON from the request body
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                // Get the latitude and longitude values from the input JSON
                double latitude = Convert.ToDouble(data.location.latitude);
                double longitude = Convert.ToDouble(data.location.longitude);

                currentLocation = new Coordinate(latitude, longitude);

                // Define the destination coordinates
                double destinationLatitude = 32.020699;
                double destinationLongitude = 34.763419;

                destination = new Coordinate(destinationLatitude, destinationLongitude);

                // Calculate the distance between the two coordinates using Geolocation package
                double distance = GeoCalculator.GetDistance(
                    originCoordinate: currentLocation,
                    destinationCoordinate: destination,
                    distanceUnit: DistanceUnit.Meters);

                // Create the output JSON
                dynamic response = new JObject();
                response.distance = distance;

                // Return the output JSON
                log.LogInformation($"Distance function: the distance is {response.distance} meters");
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                log.LogError($"Exception thrown: {ex.Message}");
                return new BadRequestObjectResult("Invalid input JSON provided.");
            }
        }
    }
}
