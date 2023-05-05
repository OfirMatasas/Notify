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
            string userName, location;
            FilterDefinition<BsonDocument> filter;
            BsonDocument document;
            ObjectResult result;
            
            log.LogInformation($"Got client's updated destination location HTTP request");

            database = AzureDatabaseClient.Instance.GetDatabase(Constants.DATABASE_NOTIFY_MTA);
            collection = database.GetCollection<BsonDocument>(Constants.COLLECTION_DESTINATION);
            
            log.LogInformation($"Got reference to {Constants.COLLECTION_DESTINATION} collection on {Constants.DATABASE_NOTIFY_MTA} database");

            requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            data = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation($"Data:{Environment.NewLine}{data}");

            try
            {
                userName = Convert.ToString(data.user);
                location = Convert.ToString(data.location.name);
                filter = Builders<BsonDocument>.Filter
                    .Where(doc => doc["user"].ToString().Equals(userName) && 
                                  doc["location"].ToString().Equals(location));
                document = await collection.Find(filter).FirstOrDefaultAsync();
                
                if (document != null)
                {
                    updateExistedDocument(data, log, document, collection, filter);
                    result = new OkObjectResult(document.ToJson());
                }
                else
                {
                    document = await createNewDocument(data, log, collection).Result;
                    result = new CreatedResult("", document);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new BadRequestObjectResult(ex.Message);
            }

            return result;
        }

        private static async void updateExistedDocument(dynamic data, ILogger log, BsonDocument document, IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter) 
        {
            log.LogInformation($"Found existing document for user {data.user}. Updating it");
            
            if (data.location.type == "Location")
            {
                document["latitude"] = Convert.ToDouble(data.location.latitude);
                document["longitude"] = Convert.ToDouble(data.location.longitude);
            }
            else if (data.location.type == "WiFi")
            {
                document["ssid"] = Convert.ToString(data.location.ssid);
            }
            else
            {
                throw new ArgumentException($"Invalid location type: {data.locationType}");
            }
            
            await collection.ReplaceOneAsync(filter, document);
            log.LogInformation("Document updated successfully");
        }

        private static async Task<BsonDocument> createNewDocument(dynamic data, ILogger log, IMongoCollection<BsonDocument> collection)
        {
            BsonDocument document;
            
            log.LogInformation($"No document found for user {data.user}. Creating a brand new one");

            document = new BsonDocument
            {
                { "user", Convert.ToString(data.user) },
                { "location", Convert.ToString(data.location.name) }
            };

            if (data.locationType == "Location")
            {
                document.Add("latitude", Convert.ToDouble(data.location.latitude));
                document.Add("longitude", Convert.ToDouble(data.location.longitude));
            }
            else if (data.locationType == "WiFi")
            {
                document.Add("ssid", Convert.ToString(data.location.ssid));
            }
            else
            {
                throw new ArgumentException($"Invalid location type: {data.locationType}");
            }
            
            log.LogInformation($"Created document:{Environment.NewLine}{document}");
            
            await collection.InsertOneAsync(document);
            log.LogInformation("Document inserted successfully");

            return document;
        }
    }
}
