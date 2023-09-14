using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notify.Functions.HTTPClients;

namespace Notify.Functions.NotifyFunctions.Google
{
    public static class GetAddressSuggestions
    {
        [FunctionName("GetAddressSuggestions")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "destination/suggestions")]
            HttpRequest request, ILogger logger)
        {
            string address;
            List<string> suggestions;
            ObjectResult result;

            logger.LogInformation("Got client's HTTP request to get suggestions for address");

            address = request.Query["address"];
            if (string.IsNullOrEmpty(address))
            {
                logger.LogError("No address provided");
                result = new BadRequestObjectResult("Please provide an address");
            }
            else
            {
                logger.LogInformation($"Address passed: {address}");

                try
                {
                    suggestions = await GoogleHttpClient.Instance.GetAddressSuggestionsAsync(address, logger);
                    
                    if (suggestions.Count.Equals(0))
                    {
                        logger.LogInformation("No suggestions found");
                        result = new NotFoundObjectResult("No suggestions found");
                    }
                    else
                    {
                        logger.LogInformation($"Suggestions found: {suggestions.Count}");
                        logger.LogInformation($"{string.Join($"{Environment.NewLine}, ", suggestions)}");
                        result = new OkObjectResult(JsonConvert.SerializeObject(suggestions));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting suggestions");
                    result = new BadRequestObjectResult(ex);
                }
            }

            return result;
        }
    }
}
