using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Notify.Functions.Core;
using Notify.Functions.HTTPClients;
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Destination
{
    public static class GetDestinations
    {
        [FunctionName("GetDestinations")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "destination")]
            HttpRequest req, ILogger log)
        {
            string lowerCasedUsername, destinations;
            ObjectResult result;

            try
            {
                if (!ValidationUtils.ValidateUsername(req, log))
                {
                    result = new BadRequestObjectResult("Invalid username provided");
                }
                else
                {
                    lowerCasedUsername = req.Query["username"].ToString().ToLower();
                    log.LogInformation($"Got client's HTTP request to get friends of user {lowerCasedUsername}");

                    destinations = await GetAllUserDestinations(lowerCasedUsername, log);
                    result = new OkObjectResult(destinations);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error getting destinations");
                result = new BadRequestObjectResult(ex);
            }

            return result;
        }

        private static async Task<string> GetAllUserDestinations(string lowerCasedUsername, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            FilterDefinition<BsonDocument> userFilter;
            List<BsonDocument> destinations;
            string response;

            log.LogInformation($"Getting all destinations of user {lowerCasedUsername}");
            
            collection = MongoUtils.GetCollection(Constants.COLLECTION_DESTINATION);
            userFilter = Builders<BsonDocument>.Filter
                .Where(doc => doc["user"].ToString().ToLower().Equals(lowerCasedUsername));
            destinations = await collection.Find(userFilter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id")).ToListAsync();
            response = Utils.ConversionUtils.ConvertBsonDocumentListToJson(destinations);

            return response;
        }
    }
}
