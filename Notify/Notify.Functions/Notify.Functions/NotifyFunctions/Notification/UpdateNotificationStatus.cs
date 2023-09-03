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
            HttpRequest req, ILogger log)
        {
            string requestBody, newStatus, response;
            List<string> notifications;
            dynamic request;
            ObjectResult result;

            log.LogInformation("Got client's HTTP request to update notification status");

            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation($"Request body:{Environment.NewLine}{requestBody}");

                request = JsonConvert.DeserializeObject<dynamic>(requestBody);
                notifications = request.notifications.ToObject<List<string>>();
                newStatus = request.status.ToObject<string>();

                response = await UpdateNotificationStatusAsync(notifications, newStatus, log);
                result = new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error updating notification status");
                result = new BadRequestObjectResult(ex);
            }

            return result;
        }

        private static async Task<string> UpdateNotificationStatusAsync(List<string> notifications, string newStatus, ILogger log)
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

            log.LogInformation($"Updating status of {notifications.Count} notifications to {newStatus}");
            result = await notificationCollection.UpdateManyAsync(
                filter: notificationsFilter, 
                update: notificationsUpdate);

            log.LogInformation($"Updated {result.ModifiedCount} notifications");

            notificationDocuments = await notificationCollection.Find(notificationsFilter).ToListAsync();
            response = ConversionUtils.ConvertBsonDocumentListToJson(notificationDocuments);

            return response;
        }
    }
}
