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
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Newsfeed
{
    public static class GetNewsfeed
    {
        [FunctionName("GetNewsfeed")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "newsfeed")] HttpRequest request, ILogger logger)
        {
            string username, json;
            
            if (string.IsNullOrEmpty(request.Query["username"]))
            {
                logger.LogError("The 'username' query parameter is required");
                return new BadRequestObjectResult("The 'username' query parameter is required.");
            }

            username = request.Query["username"];
            logger.LogInformation($"Got client's HTTP request to get newsfeed of user {username}");

            if (!ValidationUtils.ValidateUsername(request, logger))
            {
                return new BadRequestObjectResult($"The username {username} is invalid");
            }
            
            try
            {
                json = getNewsfeedByUsername(username, logger);
                return new OkObjectResult(json);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting newsfeed");
                return new BadRequestObjectResult(ex);
            }
        }
        
        private static string getNewsfeedByUsername(string username, ILogger logger)
        {
            IMongoCollection<BsonDocument> newsfeedCollection = MongoUtils.GetCollection(Constants.COLLECTION_NEWSFEED);
            FilterDefinition<BsonDocument> newsfeedFilter = Builders<BsonDocument>.Filter
                .Regex("username", new BsonRegularExpression(username, "i"));

            List<BsonDocument> newsfeeds = newsfeedCollection.FindSync(newsfeedFilter).ToList();
            logger.LogInformation($"Got {newsfeeds.Count} news of user {username}");

            if (newsfeeds.Count > 0)
            {
                // After getting the newsfeed, delete them from the database, for not getting them again
                DeleteResult deleteResult = newsfeedCollection.DeleteMany(newsfeedFilter);
                
                if(deleteResult.DeletedCount.Equals(newsfeeds.Count))
                {
                    logger.LogInformation($"Deleted {newsfeeds.Count} newsfeed of user {username}");
                }
                else
                {
                    logger.LogError($"Deleted {deleteResult.DeletedCount} newsfeed of user {username} where {newsfeeds.Count} were expected");
                }
            }

            return ConversionUtils.ConvertBsonDocumentListToJson(newsfeeds);
        }
    }
}
