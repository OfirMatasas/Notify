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

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class GetNotifications
    {
        [FunctionName("GetNotifications")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notification")]
            HttpRequest req, ILogger log)
        {
            string username, notifications;
            ObjectResult result;

            if (!ValidationUtils.ValidateUsername(req, log))
            {
                result = new BadRequestObjectResult("Invalid username provided");
            }
            else
            {
                username = req.Query["username"].ToString().ToLower();
                log.LogInformation($"Got client's HTTP request to get notifications of user {username}");

                try
                {
                    notifications = await GetAllUserNotifications(username, log);
                    result = new OkObjectResult(notifications);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error getting notifications");
                    result = new BadRequestObjectResult(ex);
                }
            }

            return result;
        }

        private static async Task<string> GetAllUserNotifications(string username, ILogger log)
        {
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);
            FilterDefinition<BsonDocument> userFilter = Builders<BsonDocument>.Filter
                .Where(doc => doc["user"].ToString().ToLower().Equals(username));
            List<BsonDocument> notifications;
            string response;

            log.LogInformation($"Getting all notifications of user {username}");

            notifications = await collection.Find(userFilter).ToListAsync();
            response = ConversionUtils.ConvertBsonDocumentListToJson(notifications);

            return response;
        }
    }
}
