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
    public static class GetPermissions
    {
        [FunctionName("GetPermissions")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "permission")]
            HttpRequest req, ILogger logger)
        {
            string lowerCasedUsername;
            List<BsonDocument> documents;
            ActionResult result;
            
            if (!ValidationUtils.ValidateUsername(req, logger))
            {
                result = new BadRequestObjectResult("Invalid username provided");
            }
            else
            {
                lowerCasedUsername = req.Query["username"].ToString().ToLower();
                documents = await getPermissionDocumentsAsync(lowerCasedUsername, logger);

                if (documents.Count.Equals(0))
                {
                    logger.LogError($"No permissions found for {lowerCasedUsername}");
                    result = new NotFoundObjectResult($"No permissions found for {lowerCasedUsername}");
                }
                else
                {
                    logger.LogInformation($"Found {documents.Count} permission documents for {lowerCasedUsername}");
                    result = new OkObjectResult(ConversionUtils.ConvertBsonDocumentListToJson(documents));
                }
            }

            return result;
        }
        
        private static async Task<List<BsonDocument>> getPermissionDocumentsAsync(string lowerCasedUsername, ILogger logger)
        {
            IMongoCollection<BsonDocument> collection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                .Where(doc => doc["permit"].ToString().ToLower().Equals(lowerCasedUsername));

            logger.LogInformation($"Getting permissions for {lowerCasedUsername}");

            return await collection.Find(filter).ToListAsync();
        }
    }
}
