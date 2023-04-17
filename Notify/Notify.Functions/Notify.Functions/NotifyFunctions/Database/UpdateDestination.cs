using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver; 
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;

namespace Notify.Functions.NotifyFunctions.Database
{
    public static class UpdateDestination
    {
        [FunctionName("UpdateDestination")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "destination/update")]
            HttpRequest req, ILogger log)
        {
            IMongoDatabase database;
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            BsonDocument newDocument;
            
            log.LogInformation($"Got client's updated destination location HTTP request");

            database = AzureDatabaseClient.Instance.GetDatabase(Constants.DATABASE_NOTIFY_MTA);
            collection = database.GetCollection<BsonDocument>(Constants.COLLECTION_DESTINATIONS);
            
            log.LogInformation($"Got reference to {Constants.COLLECTION_DESTINATIONS} collection on {Constants.DATABASE_NOTIFY_MTA} database");

            requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            data = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation($"Data:{Environment.NewLine}{data}");

            newDocument = new BsonDocument
            {
                { "user", Convert.ToString(data.user) },
                { "location", Convert.ToString(data.location.name) },
                { "latitude", Convert.ToDouble(data.location.latitude) },
                { "longitude", Convert.ToDouble(data.location.longitude) }
            };

            log.LogInformation($"Created document:{Environment.NewLine}{newDocument}");

            try
            {
                await collection.InsertOneAsync(newDocument);
                log.LogInformation("Document inserted successfully");
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
