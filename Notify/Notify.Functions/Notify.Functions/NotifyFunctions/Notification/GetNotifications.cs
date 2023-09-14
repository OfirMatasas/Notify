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
            HttpRequest request, ILogger logger)
        {
            string username, notifications;
            ObjectResult result;

            if (!ValidationUtils.ValidateUsername(request, logger))
            {
                result = new BadRequestObjectResult("Invalid username provided");
            }
            else
            {
                username = request.Query["username"].ToString().ToLower();
                logger.LogInformation($"Got client's HTTP request to get notifications of user {username}");

                try
                {
                    notifications = await GetAllUserNotifications(username, logger);
                    result = new OkObjectResult(notifications);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting notifications");
                    result = new BadRequestObjectResult(ex);
                }
            }

            return result;
        }

        private static async Task<string> GetAllUserNotifications(string username, ILogger logger)
        {
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);
            FilterDefinition<BsonDocument> userFilter = Builders<BsonDocument>.Filter
                .Where(doc => doc["user"].ToString().ToLower().Equals(username));
            List<BsonDocument> notifications;
            string response;

            logger.LogInformation($"Getting all notifications of user {username}");

            notifications = await collection.Find(userFilter).ToListAsync();
            response = ConversionUtils.ConvertBsonDocumentListToJson(notifications);

            return response;
        }
    }
}
