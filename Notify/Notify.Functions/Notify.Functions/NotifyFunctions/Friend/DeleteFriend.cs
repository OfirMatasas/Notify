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
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "friend")] HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            dynamic data;
            string username, friendName, message;
            DeleteResult deleteResult;
            ObjectResult result;

            log.LogInformation($"Got client's HTTP request to delete friend");

            collection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND);
            data = await ConversionUtils.ExtractBodyContentAsync(req);
            
            username = data.username;
            friendName = data.friendName;
            deleteResult = await deleteFriendFromDatabase(username, friendName, collection);

            if (deleteResult.DeletedCount.Equals(0))
            {
                message = $"No friendship between {username} and {friendName} was found";
                result = new NotFoundObjectResult(message);
            }
            else
            {
                message = $"Friendship and permissions between {username} and {friendName} were deleted";
                result = new OkObjectResult(message);
            }
            
            log.LogInformation(message);

            return result;
        }

        private static async Task<DeleteResult> deleteFriendFromDatabase(string username, string friendName, IMongoCollection<BsonDocument> collection)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("userName1", username),
                    Builders<BsonDocument>.Filter.Eq("userName2", friendName)),
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("userName1", friendName),
                    Builders<BsonDocument>.Filter.Eq("userName2", username)));
            DeleteResult friendshipDeleteResult = await collection.DeleteOneAsync(filter);
            DeleteResult permissionDeleteResult = null;
            
            if (!friendshipDeleteResult.DeletedCount.Equals(0))
            {
                permissionDeleteResult = await deletePermissionsFromDatabase(username, friendName);
            }
            
            return permissionDeleteResult ?? friendshipDeleteResult;
        }

        private static async Task<DeleteResult> deletePermissionsFromDatabase(string username, string friendName)
        {
            FilterDefinition<BsonDocument> filter;
            IMongoCollection<BsonDocument> permissionsCollection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);
            filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("permit", username),
                    Builders<BsonDocument>.Filter.Eq("username", friendName)),
                Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("permit", friendName),
                    Builders<BsonDocument>.Filter.Eq("username", username))
            );

            return await permissionsCollection.DeleteManyAsync(filter);
        }
    }
}
