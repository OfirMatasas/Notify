using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;

namespace Notify.Functions.NotifyFunctions.Database
{
    public static class CreateNotification
    {
        [FunctionName("CreateNotification")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notification/{type}")] HttpRequest request, ILogger log, string type)
        {
            IMongoCollection<BsonDocument> collection;
            JToken json;
            List<BsonDocument> documentsList = new List<BsonDocument>();

            log.LogInformation($"Got client's HTTP request to create notification based on {type}");

            try
            {
                getCollection(out collection);
                log.LogInformation($"Got reference to {Constants.COLLECTION_DESTINATION} collection on {Constants.DATABASE_NOTIFY_MTA} database");

                json = convertRequestBodyIntoJsonAsync(request).Result;
                log.LogInformation($"Data:{Environment.NewLine}{json}");

                createDocumentForEachUser(json, type, ref documentsList);
                log.LogInformation($"Converted JSON into {documentsList.Count} different documents");

                await collection.InsertManyAsync(documentsList);
                log.LogInformation($"{documentsList.Count} documents inserted successfully");
                
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        
        private static void getCollection(out IMongoCollection<BsonDocument> collection)
        {
            IMongoDatabase database = AzureDatabaseClient.Instance.GetDatabase(Constants.DATABASE_NOTIFY_MTA);
            collection = database.GetCollection<BsonDocument>(Constants.COLLECTION_NOTIFICATION);        
        }

        private static async Task<JToken> convertRequestBodyIntoJsonAsync(HttpRequest request)
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            
            return JToken.Parse(requestBody);        
        }

        private static void createDocumentForEachUser(JToken json, string type, ref List<BsonDocument> documentsList)
        {
            BsonDocument document;
            BsonElement extraElement;
            
            setExtraElementBaseOnType(type, json, out extraElement);

            foreach (string user in json["users"]?.ToObject<List<string>>()!)
            {
                document = new BsonDocument
                {
                    { "creator", json["creator"].ToString() },
                    { "creation_timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "status", "new" },
                    { "info", json["info"].ToString() },
                    {
                        "notification", new BsonDocument
                        {
                            { "name", json["notification"]["name"].ToString() },
                            { "type", json["notification"]["type"].ToString() },
                            extraElement
                        }
                    },
                    { "user", user }
                };

                documentsList.Add(document);
            }
        }

        private static void setExtraElementBaseOnType(string type, JToken json, out BsonElement extraElement)
        {
            if (type.Equals("location"))
            {
                extraElement = new BsonElement("location", json["notification"]["location"].ToString());
            }
            else
            {
                extraElement = new BsonElement("timestamp", int.Parse(json["notification"]["timestamp"].ToString()));
            }
        }
    }
}
