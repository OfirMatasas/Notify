using System;
using System.Collections.Generic;
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

namespace Notify.Functions.NotifyFunctions.Permission
{
    public static class UpdatePermission
    {
        [FunctionName("UpdatePermission")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "permission")]
            HttpRequest request, ILogger logger)
        {
            dynamic data = await ConversionUtils.ExtractBodyContentAsync(request);
            ActionResult result;

            logger.LogInformation($"Got client's HTTP request to update permission");

            if (data.permit == null || data.username == null)
            {
                result = new BadRequestObjectResult("Invalid request body: permit and username are required");
            }
            else if (data.location == null && data.dynamic == null && data.time == null)
            {
                result = new BadRequestObjectResult("Invalid request body: location, dynamic or time are required");
            }
            else
            {
                result = await updatePermissionAsync(data, logger);
            }

            return result;
        }

        private static async Task<ActionResult> updatePermissionAsync(dynamic data, ILogger log)
        {
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("permit", data.permit.ToString()),
                Builders<BsonDocument>.Filter.Eq("username", data.username.ToString()));
            BsonDocument permissionDocument = await collection.FindAsync(filter).Result.FirstOrDefaultAsync();
            List<UpdateDefinition<BsonDocument>> updates;
            ActionResult result;

            if (permissionDocument == null)
            {
                string errorMessage = $"No permission found for {data.permit} and {data.username}";
                log.LogError(errorMessage);
                result = new NotFoundObjectResult(errorMessage);
            }
            else
            {
                updates = CollectUpdates(data);
                if (updates.Count.Equals(0))
                {
                    result = new BadRequestObjectResult("No valid updates provided in request.");
                }
                else
                {
                    await collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Combine(updates));
                    result = new OkObjectResult($"Permission for {data.permit} and {data.username} was updated");
                }
            }
            
            return result;
        }
        
        private static List<UpdateDefinition<BsonDocument>> CollectUpdates(dynamic data)
        {
            List<UpdateDefinition<BsonDocument>> updates = new List<UpdateDefinition<BsonDocument>>();
            UpdateDefinitionBuilder<BsonDocument> updateBuilder = Builders<BsonDocument>.Update;

            Dictionary<string, object> attributes = new Dictionary<string, object>
            {
                { Constants.NOTIFICATION_TYPE_LOCATION_LOWER, data.location },
                { Constants.NOTIFICATION_TYPE_DYNAMIC_LOWER, data.dynamic },
                { Constants.NOTIFICATION_TYPE_TIME_LOWER, data.time }
            };

            foreach (KeyValuePair<string, object> attribute in attributes)
            {
                if (attribute.Value != null)
                {
                    if (IsValidPermission(attribute.Value.ToString()))
                    {
                        updates.Add(updateBuilder.Set(attribute.Key, attribute.Value.ToString()));
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid request body for {attribute.Key}: permission value must be either '{Constants.PERMISSION_ALLOW}' or '{Constants.PERMISSION_DISALLOW}'");
                    }
                }
            }

            return updates;
        }

        private static bool IsValidPermission(string permission)
        {
            return permission.Equals(Constants.PERMISSION_ALLOW) || permission.Equals(Constants.PERMISSION_DISALLOW);
        }
    }
}
