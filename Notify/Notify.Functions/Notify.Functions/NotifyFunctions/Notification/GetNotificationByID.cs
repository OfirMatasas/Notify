using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Notify.Functions.Core;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class GetNotificationByID
    {
        [FunctionName("GetNotificationByID")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notification/{id}")] HttpRequest req, 
            ILogger log, string id)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to get notification by ID: {id}");

            if (string.IsNullOrEmpty(id))
            {
                log.LogError("ID is missing");
                return new BadRequestObjectResult("Please pass an ID on the route");
            }

            try
            {
                BsonDocument notification = await GetNotificationByIdAsync(id, log);
                
                if (notification == null)
                {
                    return new NotFoundObjectResult($"Notification with ID {id} not found.");
                }

                string jsonResponse = notification.ToJson();
                return new OkObjectResult(jsonResponse);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error getting notification by ID");
                return new BadRequestObjectResult(ex);
            }
        }

        private static async Task<BsonDocument> GetNotificationByIdAsync(string id, ILogger log)
        {
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);  // Replace with your collection name
            FilterDefinition<BsonDocument> idFilter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));

            log.LogInformation($"Getting notification with ID: {id}");

            BsonDocument notification = await collection.Find(idFilter).FirstOrDefaultAsync();

            return notification;
        }
    }
}
