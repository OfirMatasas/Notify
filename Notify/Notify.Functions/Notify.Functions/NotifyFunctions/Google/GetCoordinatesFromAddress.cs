using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Functions.HTTPClients;

namespace Notify.Functions.NotifyFunctions.Google
{
    public static class GetCoordinatesFromAddress
    {
        [FunctionName("GetCoordinatesFromAddress")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "destination/coordinates")]
            HttpRequest req, ILogger log)
        {
            string address;
            GoogleHttpClient.Coordinates coordinates;
            dynamic responseJson;
            ObjectResult result;

            log.LogInformation("Got client's HTTP request to get coordinates for address");

            address = req.Query["address"];
            if (string.IsNullOrEmpty(address))
            {
                log.LogError("No address provided");
                result = new BadRequestObjectResult("Please provide an address");
            }
            else
            {
                log.LogInformation($"Address passed: {address}");

                try
                {
                    coordinates = await GoogleHttpClient.Instance.GetCoordinatesFromAddressAsync(address, log);

                    if (coordinates.Equals(null))
                    {
                        log.LogError("No coordinates found");
                        result = new NotFoundObjectResult($"No coordinates found for {address}");
                    }
                    else
                    {
                        responseJson = new JObject
                        {
                            { "longitude", coordinates.Lng },
                            { "latitude", coordinates.Lat }
                        };
                        
                        result = new OkObjectResult(JsonConvert.SerializeObject(responseJson));
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error getting coordinates");
                    result = new BadRequestObjectResult(ex);
                }
            }

            return result;
        }
    }
}
