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
using Notify.Functions.HTTPClients;

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

            try
            {
                log.LogInformation($"Received request to get user profile for username: {userName}");

                collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA,
                    collectionName: Constants.COLLECTION_USER);
                
                userFilter = Builders<BsonDocument>.Filter.Eq("userName", userName);
                user = await collection.Find(userFilter).FirstOrDefaultAsync();

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
                result = new BadRequestObjectResult($"Failed to get user details.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }
    }
}
