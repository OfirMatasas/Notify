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
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class RenewNotification
    {
        [FunctionName("RenewNotification")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "notification/renew")]
            HttpRequest request, ILogger logger)
        {
            string notificationID, creator;
            dynamic json;
            ActionResult result;

            logger.LogInformation($"Got client's HTTP request to renew notification");

            try
            {
                json = await ConversionUtils.ExtractBodyContentAsync(request);
                notificationID = json.id;
                creator = json.creator;

                logger.LogInformation($"User {creator} requested to renew notification {notificationID}");

                if (!await ValidationUtils.CheckIfUserExistsAsync(creator))
                {
                    result = new BadRequestObjectResult($"User {creator} does not exist");
                }
                else
                {
                    result = await renewNotificationAsync(notificationID, creator, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error renewing notification");
                result = new BadRequestObjectResult(ex);
            }

            return result;
        }

        private static async Task<ActionResult> renewNotificationAsync(string notificationID, string creator, ILogger logger)
        {
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);
            FilterDefinition<BsonDocument> filter =
                Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(notificationID));
            BsonDocument notificationDocument = await collection.Find(filter).FirstOrDefaultAsync();
            BsonDocument renewedNotificationDocument;
            ActionResult result;

            if (notificationDocument == null)
            {
                logger.LogError($"Notification {notificationID} does not exist");
                result = new NotFoundResult();
            }
            else
            {
                renewedNotificationDocument = new BsonDocument
                {
                    { "creator", creator },
                    { "creation_timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "status", "Active" },
                    { "description", notificationDocument["description"] },
                    {
                        "notification", new BsonDocument
                        {
                            { "name", notificationDocument["notification"]["name"] },
                            { "type", notificationDocument["notification"]["type"] },
                            { "location", notificationDocument["notification"]["location"] },
                            { "activation", notificationDocument["notification"]["activation"] },
                            { "permanent", notificationDocument["notification"]["permanent"] }
                        }
                    },
                    { "user", notificationDocument["user"] }
                };
                
                logger.LogInformation($"renewedNotificationDocument:{Environment.NewLine}{renewedNotificationDocument}");

                await collection.InsertOneAsync(renewedNotificationDocument);
                logger.LogInformation($"Renewed notification {notificationID}");

                result = new OkObjectResult(renewedNotificationDocument["_id"].ToString());
            }

            return result;
        }
    }
}
