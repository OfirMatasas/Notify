using System;
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
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Users
{
    public static class GetUserByUserName
    {
        [FunctionName("GetUserByUserName")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{userName}")]
            HttpRequest req, string userName, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            FilterDefinition<BsonDocument> userFilter;
            BsonDocument user;
            ObjectResult result;
            ProjectionDefinition<BsonDocument> projection;

            try
            {
                log.LogInformation($"Received request to get user profile for username: {userName}");

                collection = MongoUtils.GetCollection(Constants.COLLECTION_USER);
                userFilter = Builders<BsonDocument>.Filter.Eq("userName", userName);
                projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("password");
                user = await collection.Find(userFilter).Project(projection).FirstOrDefaultAsync();

                if (user != null)
                {
                    log.LogInformation($"Successfully retrieved user profile for username: {userName}");
                    result = new OkObjectResult(user.ToJson());
                }
                else
                {
                    log.LogInformation($"No user found with username: {userName}");
                    result = new NotFoundObjectResult($"Username '{userName}' not found");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to get user details. Reason: {ex.Message}");
                result = new BadRequestObjectResult(
                    $"Failed to get user details.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }
    }
}
