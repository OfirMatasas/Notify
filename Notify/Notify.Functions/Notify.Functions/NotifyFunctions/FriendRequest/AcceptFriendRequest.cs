using System;
using System.Text.RegularExpressions;
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
            HttpRequest request, ILogger logger)
        {
            string requester, username;
            ObjectResult result;
            dynamic data;

            try
            {
                data = await ConversionUtils.ExtractBodyContentAsync(request);
                requester = Convert.ToString(data.requester);
                username = Convert.ToString(data.userName);
                
                if(!checkIfFriendshipRequestExists(requester, username, logger))
                {
                    throw new Exception($"Friendship request does not exist between {requester} and {username}");
                }
                
                logger.LogInformation($"Accepting friend request from {requester} to {username}");
                await createFriendshipAsync(requester, username, logger);
                await createPermissionsAsync(requester, username, logger);
                await deleteFriendRequestAsync(requester, username, logger);
                await createNewsfeedAsync(requester, username, logger);
                
                result = new OkObjectResult("Friend request accepted");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error accepting friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return result;
        }

        private static bool checkIfFriendshipRequestExists(string requester, string username, ILogger logger)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection =
                MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);
            FilterDefinition<BsonDocument> friendRequestsFilter =  Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Regex("requester", 
                    new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i")),
                Builders<BsonDocument>.Filter.Regex("userName", 
                    new BsonRegularExpression($"^{Regex.Escape(username)}$", "i"))
            );
            
            logger.LogInformation($"Checking if friendship request exists between {requester} and {username}");
            return friendRequestsCollection.Find(friendRequestsFilter).Any();
        }

        private static async Task createPermissionsAsync(string requester, string username, ILogger log)
        {
            IMongoCollection<BsonDocument> permissionsCollection = MongoUtils.GetCollection(Constants.COLLECTION_PERMISSION);

            await createAndInsertPermissionDocument(requester, username, permissionsCollection, log);
            await createAndInsertPermissionDocument(username, requester, permissionsCollection, log);
        }

        private static async Task createAndInsertPermissionDocument(string permit, string username, IMongoCollection<BsonDocument> collection, ILogger logger)
        {
            BsonDocument permissionsDocument = new BsonDocument
            {
                { "permit", permit },
                { "username", username },
                { Constants.NOTIFICATION_TYPE_LOCATION_LOWER, Constants.PERMISSION_DISALLOW },
                { Constants.NOTIFICATION_TYPE_DYNAMIC_LOWER, Constants.PERMISSION_DISALLOW },
                { Constants.NOTIFICATION_TYPE_TIME_LOWER, Constants.PERMISSION_DISALLOW }
            };

            logger.LogInformation($"Creating permissions document for {permit} and {username}");
            await collection.InsertOneAsync(permissionsDocument);
            logger.LogInformation($"Permissions document for {permit} and {username} created successfully");
        }

        private static async Task createFriendshipAsync(string requester, string username, ILogger logger)
        {
            IMongoCollection<BsonDocument> friendsCollection;
            BsonDocument friendDocument;
            
            friendDocument = new BsonDocument
                {
                    { "userName1", requester },
                    { "userName2", username }
                };

            logger.LogInformation($"Creating friendship document between {requester} and {username}");
            
            friendsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND);
            await friendsCollection.InsertOneAsync(friendDocument);

            logger.LogInformation($"Friendship document between {requester} and {username} created successfully");
        }
        
        private static async Task deleteFriendRequestAsync(string requester, string username, ILogger logger)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            FilterDefinition<BsonDocument> friendRequestsFilter;

            friendRequestsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);
            
            friendRequestsFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Regex("requester", 
                    new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i")),
                Builders<BsonDocument>.Filter.Regex("userName", 
                    new BsonRegularExpression($"^{Regex.Escape(username)}$", "i"))
            );

            logger.LogInformation($"Deleting the pending friend request document from {requester} to {username}");

            await friendRequestsCollection.DeleteOneAsync(friendRequestsFilter);

            logger.LogInformation($"Friend request document between {requester} and {username} deleted successfully");
        }
        
        private static async Task createNewsfeedAsync(string requester, string username, ILogger logger)
        {
            IMongoCollection<BsonDocument> newsfeedCollection;
            BsonDocument newsfeedDocument;
            
            newsfeedDocument = new BsonDocument
            {
                { "username", requester },
                { "title", "New Friend Approval" },
                { "content", $"{username} has accepted your friend request" }
            };

            logger.LogInformation($"Creating newsfeed document for {username} and {requester}");
            
            newsfeedCollection = MongoUtils.GetCollection(Constants.COLLECTION_NEWSFEED);
            await newsfeedCollection.InsertOneAsync(newsfeedDocument);

            logger.LogInformation($"Newsfeed document for {username} and {requester} created successfully");
        }
    }
}
