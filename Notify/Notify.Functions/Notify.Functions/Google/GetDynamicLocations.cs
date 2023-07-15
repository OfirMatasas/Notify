using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;

namespace Notify.Functions.Google
{
    public static class GetDynamicLocations
    {
        [FunctionName("GetDynamicLocations")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "destination/dynamic")]
            HttpRequest req, ILogger log)
        {
            string requestBody;
            dynamic data;
            double longitude, latitude;
            string type;
            List<Place> results;

            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
                log.LogInformation($"Data:{Environment.NewLine}{data}");
                
                longitude = Convert.ToDouble(data.longitude);
                latitude = Convert.ToDouble(data.latitude);
                type = Convert.ToString(data.type);

                log.LogInformation($"Got client's HTTP request to get dynamic locations of type {type} near {longitude}, {latitude}");
                
                results = await GoogleHttpClient.Instance.SearchPlacesNearby(
                    longitude: longitude, 
                    latitude: latitude, 
                    radius: Constants.GOOGLE_API_RADIUS, 
                    type: type);

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error getting dynamic locations");
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
