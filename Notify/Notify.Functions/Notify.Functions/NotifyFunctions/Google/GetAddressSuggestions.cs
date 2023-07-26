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
            HttpRequest req, ILogger log)
        {
            string address;
            List<string> suggestions;
            ObjectResult result;

            log.LogInformation("Got client's HTTP request to get suggestions for address");

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
                    suggestions = await GoogleHttpClient.Instance.GetAddressSuggestionsAsync(address, log);
                    
                    if (suggestions.Count.Equals(0))
                    {
                        log.LogInformation("No suggestions found");
                        result = new NotFoundObjectResult("No suggestions found");
                    }
                    else
                    {
                        log.LogInformation($"Suggestions found: {suggestions.Count}");
                        log.LogInformation($"{string.Join($"{Environment.NewLine}, ", suggestions)}");
                        result = new OkObjectResult(JsonConvert.SerializeObject(suggestions));
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error getting suggestions");
                    result = new BadRequestObjectResult(ex);
                }
            }

            return result;
        }
    }
}
