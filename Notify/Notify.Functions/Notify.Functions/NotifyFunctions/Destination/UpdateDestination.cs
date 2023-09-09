using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver; 
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Notify.Functions.Core;
using Notify.Functions.HTTPClients;
using Notify.Functions.Utils;
using static MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Destination
{
    public static class UpdateDestination
    {
        [FunctionName("UpdateDestination")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "destination/update")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            dynamic data;
            string userName, locationName;
            FilterDefinition<BsonDocument> filter;
            BsonDocument document;
            ObjectResult result;
            
            log.LogInformation($"Got client's updated destination location HTTP request");
            
            data = await ConversionUtils.ExtractBodyContentAsync(req);
            log.LogInformation($"Data:{Environment.NewLine}{data}");

            try
            {
                userName = Convert.ToString(data.user);
                locationName = Convert.ToString(data.location.name);
                
                log.LogInformation($"Searching for existing document in database by user {userName} and location {locationName}");
                
                filter = Filter.And(
                    Filter.Eq("user", userName), 
                    Filter.Eq("location.name", locationName)
                );
                
                collection = MongoUtils.GetCollection(Constants.COLLECTION_DESTINATION);
                document = await collection.Find(filter).FirstOrDefaultAsync();
                
                if (document != null)
                {
                    updateExistedDocument(data, log, document, collection, filter);
                    result = new OkObjectResult(document.ToJson());
                }
                else
                {
                    document = await createNewDocument(data, log, collection);
                    result = new CreatedResult("", document.ToJson());
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new BadRequestObjectResult(ex.Message);
            }

            return result;
        }
        
        private static bool shouldRemoveDestination(string type, dynamic data)
        {
            bool shouldRemoveDestination, shouldRemoveLocation, shouldRemoveWifi, shouldRemoveBluetooth;

            shouldRemoveLocation = type.Equals(Constants.NOTIFICATION_TYPE_LOCATION) &&
                                   (data.location.longitude == null && data.location.latitude == null);
            shouldRemoveWifi = type.Equals(Constants.NOTIFICATION_TYPE_WIFI) && data.location.ssid == null;
            shouldRemoveBluetooth = type.Equals(Constants.NOTIFICATION_TYPE_BLUETOOTH) && data.location.device == null;

            shouldRemoveDestination = shouldRemoveLocation || shouldRemoveWifi || shouldRemoveBluetooth;
            
            return shouldRemoveDestination;
        }

        private static async void updateExistedDocument(dynamic data, ILogger log, BsonDocument document, IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter) 
        {
            string type = Convert.ToString(data.location.type);
            
            log.LogInformation($"Found existing document for user {data.user} and location {data.location.name}. Updating it");

            if (type.Equals(Constants.NOTIFICATION_TYPE_LOCATION))
            {
                if (data.location.longitude == null && data.location.latitude == null)
                {
                    document["location"].AsBsonDocument.Remove("latitude");
                    document["location"].AsBsonDocument.Remove("longitude");
                    document["location"].AsBsonDocument.Remove("address");
                }
                else
                {
                    double latitude = Convert.ToDouble(data.location.latitude), longitude = Convert.ToDouble(data.location.longitude);
                    string address = GoogleHttpClient.Instance.GetAddressFromCoordinatesAsync(latitude, longitude, log).Result;

                    document["location"].AsBsonDocument["latitude"] = latitude;
                    document["location"].AsBsonDocument["longitude"] = longitude;
                    document["location"].AsBsonDocument["address"] = address;
                }
            }
            else if (type.Equals(Constants.NOTIFICATION_TYPE_WIFI))
            {
                if (data.location.ssid == null)
                {
                    document["location"].AsBsonDocument.Remove("ssid");
                }
                else
                {
                    document["location"].AsBsonDocument["ssid"] = Convert.ToString(data.location.ssid);
                }
            }
            else if (type.Equals(Constants.NOTIFICATION_TYPE_BLUETOOTH))
            {
                if (data.location.device == null)
                    document["location"].AsBsonDocument.Remove("device");
                else
                    document["location"].AsBsonDocument["device"] = Convert.ToString(data.location.device);
            }
            else
            {
                throw new ArgumentException($"Invalid location type: {type}");
            }
            
            await collection.ReplaceOneAsync(filter, document);
            log.LogInformation("Document updated successfully");
        }

        private static async Task<BsonDocument> createNewDocument(dynamic data, ILogger log, IMongoCollection<BsonDocument> collection)
        {
            BsonDocument document;
            string type = Convert.ToString(data.location.type);

            log.LogInformation($"No document found for user {data.user} and location {data.location.name}. Creating a brand new one");

            document = new BsonDocument
            {
                { "user", Convert.ToString(data.user) },
                { "location", new BsonDocument
                    {
                        { "name", Convert.ToString(data.location.name) }
                    }
                }
            };

            if (type.Equals(Constants.NOTIFICATION_TYPE_LOCATION))
            {
                double latitude = Convert.ToDouble(data.location.latitude), longitude = Convert.ToDouble(data.location.longitude);
                string address = GoogleHttpClient.Instance.GetAddressFromCoordinatesAsync(latitude, longitude, log).Result;
                
                document["location"].AsBsonDocument.Add("latitude", latitude);
                document["location"].AsBsonDocument.Add("longitude", longitude);
                document["location"].AsBsonDocument.Add("address", address);
            }
            else if (type.Equals(Constants.NOTIFICATION_TYPE_WIFI))
            {
                document["location"].AsBsonDocument.Add("ssid", Convert.ToString(data.location.ssid));
            }
            else if (type.Equals(Constants.NOTIFICATION_TYPE_BLUETOOTH))
            {
                document["location"].AsBsonDocument.Add("device", Convert.ToString(data.location.device));
            }
            else
            {
                throw new ArgumentException($"Invalid location type: {type}");
            }
            
            log.LogInformation($"Created document:{Environment.NewLine}{document}");
            
            await collection.InsertOneAsync(document);
            log.LogInformation("Document inserted successfully");

            return document;
        }
    }
}
