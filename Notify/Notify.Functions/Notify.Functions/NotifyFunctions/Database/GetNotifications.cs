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
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;

namespace Notify.Functions.NotifyFunctions.Database;

public static class GetNotifications
{
    [FunctionName("GetNotifications")]
    [AllowAnonymous]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notification")] HttpRequest req, ILogger log)
    {
        IMongoCollection<BsonDocument> collection;
        List<BsonDocument> notifications;
        List<BsonDocument> cleanedNotifications = new List<BsonDocument>();

        log.LogInformation("Got client's HTTP request to get notifications");

        try
        {
            collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA, Constants.COLLECTION_NOTIFICATION);
            log.LogInformation($"Got reference to {Constants.COLLECTION_NOTIFICATION} collection on {Constants.DATABASE_NOTIFY_MTA} database");

            notifications = collection.Find("{}").ToList();

            foreach (BsonDocument notification in notifications)
            {
                notification.Remove("_id");
                cleanedNotifications.Add(notification);
            }

            if (cleanedNotifications.Count == 0)
            {
                throw new Exception("There are no notifications in the database");
            }
            
            var json = cleanedNotifications.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            
            return new OkObjectResult(json);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error getting notifications");
            return new BadRequestObjectResult(ex);
        }
    }
}
