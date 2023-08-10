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
using Notify.Functions.Utils;

namespace Notify.Functions.NotifyFunctions.Destinations
{
    public static class DeleteDestination
    {
        [FunctionName("DeleteDestination")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "destination/delete")]
            HttpRequest req, ILogger log)
        {
            IMongoDatabase database;
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            DeleteResult result;

            log.LogInformation($"Got client's HTTP request to delete destination");

            database = AzureDatabaseClient.Instance.GetDatabase(Constants.DATABASE_NOTIFY_MTA);
            collection = database.GetCollection<BsonDocument>(Constants.COLLECTION_DESTINATION);
            
            log.LogInformation($"Got reference to {Constants.COLLECTION_DESTINATION} collection on {Constants.DATABASE_NOTIFY_MTA} database");
            
            data = await ConversionUtils.ExtractBodyContent(req);;
            log.LogInformation($"Data:{Environment.NewLine}{data}");
            
            filter = Builders<BsonDocument>.Filter.Eq("user", Convert.ToString(data.user)) & 
                     Builders<BsonDocument>.Filter.Eq("location.name", Convert.ToString(data.location));
            
            result = collection.DeleteMany(filter);
            log.LogInformation($"Deleted {result.DeletedCount} documents");

            return new OkResult();
        }
    }
}
