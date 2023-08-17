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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notification/create/{type}")]
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
            string creator = Convert.ToString(json["creator"]);

            setExtraElementBaseOnType(type, json, out extraElement);

            foreach (string user in json["users"]?.ToObject<List<string>>()!)
            {
                setStatusForDocument(creator, user, type, out string status);

                document = new BsonDocument
                {
                    { "creator", creator },
                    { "creation_timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "status", status },
                    { "description", Convert.ToString(json["description"]) },
                    {
                        "notification", new BsonDocument
                        {
                            { "name", Convert.ToString(json["notification"]["name"]) },
                            { "type", Convert.ToString(json["notification"]["type"]) },
                            extraElement
                        }
                    },
                    { "user", user }
                };

                if (type.ToLower().Equals(Constants.NOTIFICATION_TYPE_LOCATION.ToLower()))
                {
                    document["notification"]["activation"] = Convert.ToString(json["notification"]["activation"]);
                    document["notification"]["permanent"] = Convert.ToString(json["notification"]["permanent"]);
                }

                documentsList.Add(document);
            }
        }

        private static void setStatusForDocument(string creator, string user, string type, out string status)
        {
            IMongoCollection<BsonDocument> permissionCollection;
            FilterDefinition<BsonDocument> permissionFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("permit", user),
                Builders<BsonDocument>.Filter.Eq("username", creator));
            BsonDocument permissionDocument;
            string relevantPermission;

            if (creator.Equals(user))
            {
                status = Constants.NOTIFICATION_STATUS_ACTIVE;
            }
            else
            {
                permissionCollection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);
                permissionDocument = permissionCollection.Find(permissionFilter).FirstOrDefault();

                if (permissionDocument != null)
                {
                    relevantPermission = Convert.ToString(permissionDocument[type.ToLower()]) ?? Constants.PERMISSION_DISALLOW;
                    status = relevantPermission.Equals(Constants.PERMISSION_ALLOW) ? Constants.NOTIFICATION_STATUS_ACTIVE : Constants.NOTIFICATION_STATUS_PENDING;
                }
                else
                {
                    throw new ArgumentException($"There's no permission document for permit {user} and username {creator}");
                }
            }
        }

        private static void setExtraElementBaseOnType(string type, JToken json, out BsonElement extraElement)
        {
            string lowerCasedType = type.ToLower();
            
            if (lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_LOCATION.ToLower())
                || lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_DYNAMIC.ToLower()))
            {
                extraElement = new BsonElement("location", json["notification"]["location"].ToString());
            }
            else if(lowerCasedType.Equals(Constants.NOTIFICATION_TYPE_TIME.ToLower()))
            {
                extraElement = new BsonElement("timestamp", int.Parse(json["notification"]["timestamp"].ToString()));
            }
            else
            {
                throw new ArgumentException($"Type {type} is not supported");
            }
        }
    }
}
