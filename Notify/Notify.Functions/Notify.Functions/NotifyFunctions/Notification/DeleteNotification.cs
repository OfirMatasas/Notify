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
    public static class DeleteNotification
    {
        [FunctionName("DeleteNotification")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "notification")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            DeleteResult deleteResult;
            ActionResult result;

            log.LogInformation($"Got client's HTTP request to delete notifications");

            collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);
            data = await ConversionUtils.ExtractBodyContentAsync(req);

            log.LogInformation($"Data:{Environment.NewLine}{data}");

            if(null != data.user)
            {
                filter = Builders<BsonDocument>.Filter.Eq("user", Convert.ToString(data.user));
                deleteResult = collection.DeleteMany(filter);
            }
            else
            {
                filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(Convert.ToString(data.id)));
                deleteResult = collection.DeleteOne(filter);
            }

            if (deleteResult.DeletedCount.Equals(0))
            {
                log.LogError("No documents were deleted");
                result = new NotFoundResult();
            }
            else
            {
                log.LogInformation($"Deleted {deleteResult.DeletedCount} documents");
                result = new OkResult();
            }

            return result;
        }
    }
}
