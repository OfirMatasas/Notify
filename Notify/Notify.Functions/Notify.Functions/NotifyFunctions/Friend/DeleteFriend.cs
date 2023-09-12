using System.Text.RegularExpressions;
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

namespace Notify.Functions.NotifyFunctions.Friend
{
    public static class DeleteFriend
    {
        [FunctionName("DeleteFriend")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "friend")] 
            HttpRequest request, ILogger logger)
        {
            IMongoCollection<BsonDocument> collection;
            dynamic data;
            string username, friendUsername, message;
            DeleteResult deleteResult;
            ObjectResult result;

            logger.LogInformation($"Got client's HTTP request to delete friend");

            collection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND);
            data = await ConversionUtils.ExtractBodyContentAsync(request);
            
            username = data.username;
            friendUsername = data.friendUsername;
            deleteResult = await deleteFriendFromDatabase(username, friendUsername, collection);

            if (deleteResult.DeletedCount.Equals(0))
            {
                message = $"No friendship between {username} and {friendUsername} was found";
                result = new NotFoundObjectResult(message);
            }
            else
            {
                message = $"Friendship and permissions between {username} and {friendUsername} were deleted";
                result = new OkObjectResult(message);
            }
            
            logger.LogInformation(message);

            return result;
        }

        private static async Task<DeleteResult> deleteFriendFromDatabase(string username, string friendUsername, IMongoCollection<BsonDocument> collection)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Regex("userName1", 
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("userName2", 
                        new BsonRegularExpression($"^{Regex.Escape(friendUsername)}$", "i"))),
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Regex("userName1", 
                        new BsonRegularExpression($"^{Regex.Escape(friendUsername)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("userName2", 
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i"))));
            DeleteResult friendshipDeleteResult = await collection.DeleteManyAsync(filter);
            DeleteResult permissionDeleteResult = null;
            
            if (!friendshipDeleteResult.DeletedCount.Equals(0))
            {
                permissionDeleteResult = await deletePermissionsFromDatabase(username, friendUsername);
            }
            
            return permissionDeleteResult ?? friendshipDeleteResult;
        }

        private static async Task<DeleteResult> deletePermissionsFromDatabase(string username, string friendUsername)
        {
            FilterDefinition<BsonDocument> filter;
            IMongoCollection<BsonDocument> permissionsCollection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);
            filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Regex("permit", 
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("username", 
                        new BsonRegularExpression($"^{Regex.Escape(friendUsername)}$", "i"))),
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Regex("permit", 
                        new BsonRegularExpression($"^{Regex.Escape(friendUsername)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("username", 
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")))
            );

            return await permissionsCollection.DeleteManyAsync(filter);
        }
    }
}
