using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class UpdateNotificationStatus
    {
        [FunctionName("UpdateNotificationStatus")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "notification/status")]
            HttpRequest request, ILogger logger)
        {
            string requestBody, newStatus, response;
            List<string> notifications;
            dynamic requestData;
            ObjectResult result;

            logger.LogInformation("Got client's HTTP request to update notification status");

            try
            {
                requestBody = await new StreamReader(request.Body).ReadToEndAsync();
                logger.LogInformation($"Request body:{Environment.NewLine}{requestBody}");

                requestData = JsonConvert.DeserializeObject<dynamic>(requestBody);
                notifications = requestData.notifications.ToObject<List<string>>();
                newStatus = requestData.status.ToObject<string>();
                
                if (newStatus.Equals("Expired"))
                {
                    await createNewsfeedForNotificationsAsync(notifications, logger);
                }

                response = await UpdateNotificationStatusAsync(notifications, newStatus, logger);
                result = new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating notification status");
                result = new BadRequestObjectResult(ex);
            }

            return result;
        }
        
        private static async Task createNewsfeedForNotificationsAsync(List<string> notifications, ILogger logger)
        {
            IMongoCollection<BsonDocument> notificationCollection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);
            List<BsonDocument> notificationDocuments;
            IMongoCollection<BsonDocument> newsfeedCollection = MongoUtils.GetCollection(Constants.COLLECTION_NEWSFEED);
            List<BsonDocument> newsfeedDocuments = new List<BsonDocument>();
            BsonDocument newsfeedDocument;
            FilterDefinition<BsonDocument> notificationsFilter = Builders<BsonDocument>.Filter.In(
                field: "_id",
                values: notifications.Select(id => new BsonObjectId(ObjectId.Parse(id))));
            
            notificationDocuments = await notificationCollection.Find(notificationsFilter).ToListAsync();
            logger.LogInformation($"Got {notificationDocuments.Count} notifications to check for newsfeed creation");
            
            foreach (BsonDocument notificationDocument in notificationDocuments)
            {
                if (!notificationDocument["status"].AsString.Equals("Pending"))
                {
                    newsfeedDocument = new BsonDocument
                    {
                        { "username", notificationDocument["creator"] },
                        { "title", $"Notification Triggered Successfully" },
                        { "content", $"Notification {notificationDocument["notification"]["name"]} was triggered successfully by {notificationDocument["user"]}" }
                    };
                    
                    newsfeedDocuments.Add(newsfeedDocument);
                }
            }
            
            if (newsfeedDocuments.Count > 0)
            {
                await newsfeedCollection.InsertManyAsync(newsfeedDocuments);
                logger.LogInformation($"Created {newsfeedDocuments.Count} newsfeed documents");
            }
        }

        private static async Task<string> UpdateNotificationStatusAsync(List<string> notifications, string newStatus, ILogger logger)
        {
            IMongoCollection<BsonDocument> notificationCollection;
            FilterDefinition<BsonDocument> notificationsFilter;
            UpdateDefinition<BsonDocument> notificationsUpdate;
            List<BsonDocument> notificationDocuments;
            UpdateResult result;
            string response;

            notificationCollection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);

            notificationsFilter = Builders<BsonDocument>.Filter.In(
                field: "_id",
                values: notifications.Select(id => new BsonObjectId(ObjectId.Parse(id))));
            notificationsUpdate = Builders<BsonDocument>.Update.Set(
                field:"status", 
                value: newStatus);

            logger.LogInformation($"Updating status of {notifications.Count} notifications to {newStatus}");
            result = await notificationCollection.UpdateManyAsync(
                filter: notificationsFilter, 
                update: notificationsUpdate);

            logger.LogInformation($"Updated {result.ModifiedCount} notifications");

            notificationDocuments = await notificationCollection.Find(notificationsFilter).ToListAsync();
            response = ConversionUtils.ConvertBsonDocumentListToJson(notificationDocuments);

            return response;
        }
    }
}
