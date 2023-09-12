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
using Notify.Functions.Core;
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Newsfeed
{
    public static class CreateNewsfeed
    {
        [FunctionName("CreateNewsfeed")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "newsfeed")]
            HttpRequest request, ILogger logger)
        {
            dynamic data;
            string username, json;

            data = await ConversionUtils.ExtractBodyContentAsync(request);
            
            if (data.username == null)
            {
                logger.LogError("The 'username' field is required in the request body");
                return new BadRequestObjectResult("The 'username' field is required in the request body");
            }
            
            username = Convert.ToString(data.username);
            logger.LogInformation($"Got client's HTTP request to create newsfeed of user {username}");

            if (!await ValidationUtils.CheckIfUserExistsAsync(username))
            {
                return new BadRequestObjectResult($"The username {username} does not exist");
            }
            
            try
            {
                json = createNewsfeed(data, logger);
                return new OkObjectResult(json);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating newsfeed");
                return new BadRequestObjectResult(ex);
            }
        }

        private static string createNewsfeed(dynamic data, ILogger logger)
        {
            IMongoCollection<BsonDocument> newsfeedCollection = MongoUtils.GetCollection(Constants.COLLECTION_NEWSFEED);
            BsonDocument newsfeed = new BsonDocument
            {
                { "username", Convert.ToString(data.username)},
                { "title", Convert.ToString(data.title) },
                { "content", Convert.ToString(data.content) }
            };
            
            newsfeedCollection.InsertOne(newsfeed);
            logger.LogInformation($"Created newsfeed of user {data.username}{Environment.NewLine}: {newsfeed.ToJson()}");
            
            return newsfeed.ToJson();
        }
    }
}
