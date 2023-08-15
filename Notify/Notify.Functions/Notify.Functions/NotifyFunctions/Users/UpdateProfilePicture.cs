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
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Users
{
    public static class UpdateProfilePicture
    {
        [FunctionName("UpdateProfilePicture")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "post", Route = "user/profilePicture")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            UpdateDefinition<BsonDocument> update;
            ObjectResult result;
            UpdateResult updateResult;

            log.LogInformation("Received request to update user profile picture");

            try
            {
                data = await ConversionUtils.ExtractBodyContentAsync(req);

                filter = Builders<BsonDocument>.Filter.Eq("userName", Convert.ToString(data.userName));
                update = BuildUpdateDefinition(data);

                if (update == null)
                {
                    log.LogWarning("No updates provided.");
                    return new BadRequestObjectResult("No updates provided.");
                }

                collection = MongoUtils.GetCollection(Constants.COLLECTION_USER);
                updateResult = await collection.UpdateOneAsync(filter, update);

                if (updateResult.ModifiedCount > 0)
                {
                    log.LogInformation($"Successfully updated profile picture for username: {data.userName}");
                    result = new OkObjectResult(JsonConvert.SerializeObject(data));
                }
                else
                {
                    log.LogInformation($"No user found with username: {data.userName}");
                    result = new NotFoundObjectResult($"Username '{data.userName}' not found");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to update user details. Reason: {ex.Message}");
                result = new ObjectResult($"Failed to update user details.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }

        private static UpdateDefinition<BsonDocument> BuildUpdateDefinition(dynamic data)
        {
            UpdateDefinitionBuilder<BsonDocument> updateBuilder = Builders<BsonDocument>.Update;

            string profilePicture = Convert.ToString(data.profilePicture);

            if (string.IsNullOrEmpty(profilePicture))
            {
                profilePicture = Constants.BLOB_DEFAULT_PROFILE_IMAGE;
            }

            return updateBuilder.Set("profilePicture", profilePicture);
        }
    }
}
