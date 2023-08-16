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

namespace Notify.Functions.NotifyFunctions.Destination
{
    public static class DeleteDestination
    {
        [FunctionName("DeleteDestination")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "destination/delete")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            DeleteResult result;

            log.LogInformation($"Got client's HTTP request to delete destination");
            
            data = await ConversionUtils.ExtractBodyContentAsync(req);;
            log.LogInformation($"Data:{Environment.NewLine}{data}");
            
            filter = Builders<BsonDocument>.Filter.Eq("user", Convert.ToString(data.user)) & 
                     Builders<BsonDocument>.Filter.Eq("location.name", Convert.ToString(data.location));
            
            collection = MongoUtils.GetCollection(Constants.COLLECTION_DESTINATION);
            result = collection.DeleteMany(filter);
            log.LogInformation($"Deleted {result.DeletedCount} documents");

            return new OkResult();
        }
    }
}
