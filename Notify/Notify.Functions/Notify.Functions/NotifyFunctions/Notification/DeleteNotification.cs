using System;
using System.IO;
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
using Notify.Functions.HTTPClients;

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class DeleteNotification
    {
        [FunctionName("DeleteNotification")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "notification")]
            HttpRequest req, ILogger log)
        {
            IMongoDatabase database;
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            DeleteResult result;

            log.LogInformation($"Got client's HTTP request to delete notifications");

            database = AzureDatabaseClient.Instance.GetDatabase(Constants.DATABASE_NOTIFY_MTA);
            collection = database.GetCollection<BsonDocument>(Constants.COLLECTION_NOTIFICATION);
            
            log.LogInformation($"Got reference to {Constants.COLLECTION_NOTIFICATION} collection on {Constants.DATABASE_NOTIFY_MTA} database");
            
            requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            data = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation($"Data:{Environment.NewLine}{data}");

            filter = Builders<BsonDocument>.Filter.Eq("user", Convert.ToString(data.user));
            
            result = collection.DeleteMany(filter);
            log.LogInformation($"Deleted {result.DeletedCount} documents");

            return new OkResult();
        }
    }
}
