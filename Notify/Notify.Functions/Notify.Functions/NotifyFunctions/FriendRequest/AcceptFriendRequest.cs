using System;
using System.Threading.Tasks;
using System.Web.Http;
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

namespace Notify.Functions.NotifyFunctions.FriendRequest
{
    public static class AcceptFriendRequest
    {
        [FunctionName("AcceptFriendRequest")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "friend/accept")]
            HttpRequest req, ILogger log)
        {
            string requester, username;
            ObjectResult result;
            dynamic data;

            try
            {
                data = await ConversionUtils.ExtractBodyContentAsync(req);
                requester = Convert.ToString(data.requester);
                username = Convert.ToString(data.userName);
                
                log.LogInformation($"Accepting friend request from {requester} to {username}");
                await createFriendshipAsync(requester, username, log);
                await createPermissionsAsync(requester, username, log);
                await deleteFriendRequestAsync(requester, username, log);
                
                result = new OkObjectResult("Friend request accepted");
            }
            catch (Exception ex)
            {
                log.LogError($"Error accepting friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return result;
        }

        private static async Task createPermissionsAsync(string requester, string username, ILogger log)
        {
            IMongoCollection<BsonDocument> permissionsCollection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);

            await createAndInsertPermissionDocument(requester, username, permissionsCollection, log);
            await createAndInsertPermissionDocument(username, requester, permissionsCollection, log);
        }

        private static async Task createAndInsertPermissionDocument(string permit, string username, IMongoCollection<BsonDocument> collection, ILogger log)
        {
            BsonDocument permissionsDocument = new BsonDocument
            {
                { "permit", permit },
                { "username", username },
                { Constants.NOTIFICATION_TYPE_LOCATION_LOWER, Constants.PERMISSION_DISALLOW },
                { Constants.NOTIFICATION_TYPE_DYNAMIC_LOWER, Constants.PERMISSION_DISALLOW },
                { Constants.NOTIFICATION_TYPE_TIME_LOWER, Constants.PERMISSION_DISALLOW }
            };

            log.LogInformation($"Creating permissions document for {permit} and {username}");
            await collection.InsertOneAsync(permissionsDocument);
            log.LogInformation($"Permissions document for {permit} and {username} created successfully");
        }

        private static async Task createFriendshipAsync(string requester, string username, ILogger log)
        {
            IMongoCollection<BsonDocument> friendsCollection;
            BsonDocument friendDocument;
            
            friendDocument = new BsonDocument
                {
                    { "userName1", requester },
                    { "userName2", username }
                };

            log.LogInformation($"Creating friendship document between {requester} and {username}");
            
            friendsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND);
            await friendsCollection.InsertOneAsync(friendDocument);

            log.LogInformation($"Friendship document between {requester} and {username} created successfully");
        }
        
        private static async Task deleteFriendRequestAsync(string requester, string username, ILogger log)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            FilterDefinition<BsonDocument> friendRequestsFilter;

            friendRequestsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);

            friendRequestsFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("requester", requester),
                Builders<BsonDocument>.Filter.Eq("userName", username)
            );

            log.LogInformation($"Deleting the pending friend request document from {requester} to {username}");

            await friendRequestsCollection.DeleteOneAsync(friendRequestsFilter);

            log.LogInformation($"Friend request document between {requester} and {username} deleted successfully");
        }
    }
}
