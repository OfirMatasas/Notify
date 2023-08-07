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
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Notification
{
    public static class CreateNotification
    {
        [FunctionName("CreateNotification")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notification/{type}")]
            HttpRequest request, ILogger log, string type)
        {
            IMongoCollection<BsonDocument> collection;
            JToken json;
            List<BsonDocument> documentsList = new List<BsonDocument>();
            ActionResult result;

            log.LogInformation($"Got client's HTTP request to create notification based on {type}");

            try
            {
                collection = MongoUtils.GetCollection(Constants.COLLECTION_NOTIFICATION);

                json = convertRequestBodyIntoJsonAsync(request).Result;
                log.LogInformation($"Data:{Environment.NewLine}{json}");

                createDocumentForEachUser(json, type, ref documentsList);
                log.LogInformation($"Converted JSON into {documentsList.Count} different documents");

                await collection.InsertManyAsync(documentsList);
                log.LogInformation($"{documentsList.Count} documents inserted successfully");

                result = new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            
            return result;
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
                    { "status", "Active" },
                    { "description", json["description"].ToString() },
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

                if (type.ToLower().Equals("location"))
                {
                    document["notification"]["activation"] = json["notification"]["activation"].ToString();
                    document["notification"]["permanent"] = json["notification"]["permanent"].ToString();
                }

                documentsList.Add(document);
            }
        }

        private static void setExtraElementBaseOnType(string type, JToken json, out BsonElement extraElement)
        {
            string lowerCasedType = type.ToLower();
            
            if (lowerCasedType.Equals("location") || lowerCasedType.Equals("dynamic"))
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
