using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Notify.Functions.Utils;
using Constants = Notify.Functions.Core.Constants;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class UpdateNotification
    {
        [FunctionName("UpdateNotification")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "notification/update/{type}")]
            HttpRequest req, ILogger log, string type)
        {
            dynamic data = await ConversionUtils.ExtractBodyContentAsync(req);
            ActionResult result;

            log.LogInformation($"Got client's HTTP request to update notification based on {type}");
            
            try
            {
                result = await updateNotificationAsync(data, log, type);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error updating notification. Error message: {ex.Message}");
                result = new BadRequestObjectResult(ex.Message);
            }
            
            return result;
        }

        private static async Task<ActionResult> updateNotificationAsync(dynamic data, ILogger log, string type)
        {
            string lowerCasedType = type.ToLower();
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(Convert.ToString(data.id)));
            BsonDocument notificationDocument = await collection.FindAsync(filter).Result.FirstOrDefaultAsync();
            UpdateDefinitionBuilder<BsonDocument> updateBuilder = Builders<BsonDocument>.Update;
            List<UpdateDefinition<BsonDocument>> updates = new List<UpdateDefinition<BsonDocument>>();
            
            checkIfNotificationsCorrelates(data, lowerCasedType, notificationDocument);
            
            updates.Add(updateBuilder.Set("notification.name", Convert.ToString(data.name)));
            updates.Add(updateBuilder.Set("description", Convert.ToString(data.description)));

            if (lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_LOCATION_LOWER))
            {
                updates.Add(updateBuilder.Set("notification.location", Convert.ToString(data.location)));
                updates.Add(updateBuilder.Set("notification.permanent", Convert.ToString(data.permanent)));
                updates.Add(updateBuilder.Set("notification.location", Convert.ToString(data.location)));
            }
            else if (lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_DYNAMIC_LOWER))
            {
                updates.Add(updateBuilder.Set("notification.location", Convert.ToString(data.location)));
            }
            else if (lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_TIME_LOWER))
            {
                updates.Add(updateBuilder.Set("notification.timestamp", Convert.ToInt64(data.timestamp)));
            }
            else
            {
                throw new ArgumentException($"Type {type} is not supported");
            }

            log.LogInformation($"Attempting to update notification {data.id}");
            await collection.UpdateOneAsync(filter, updateBuilder.Combine(updates));
            log.LogInformation($"Successfully updated notification {data.id}");
            
            return new OkObjectResult($"Successfully updated notification {data.id}");
        }

        private static void checkIfNotificationsCorrelates(dynamic data, string lowerCasedType, BsonDocument notificationDocument)
        {
            if (notificationDocument.IsNullOrEmpty())
            {
                throw new ArgumentException($"Notification with id {data.id} does not exist");
            }
            if (!lowerCasedType.Equals(Convert.ToString(notificationDocument["notification"]["type"]).ToLower()))
            {
                throw new ArgumentException($"Notification with id {data.id} is not a type of '{lowerCasedType}'");
            }
            if(!lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_LOCATION_LOWER))
            {
                if (data.permanent != null && Convert.ToBoolean(data.permanent))
                {
                    throw new ArgumentException($"Notification with id {data.id} is not a location type, therefore cannot be permanent");
                }
                if (data.activation != null && data.activation.Equals("Leave"))
                {
                    throw new ArgumentException($"Notification with id {data.id} is not a location type, therefore cannot be activated by leaving");
                }
            }
        }
    }
}
