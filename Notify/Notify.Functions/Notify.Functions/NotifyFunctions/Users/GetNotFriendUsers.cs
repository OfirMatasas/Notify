using System.Collections.Generic;
using System.Linq;
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
using Notify.Functions.HTTPClients;
using Notify.Functions.Utils;
using Constants = Notify.Functions.Core.Constants;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Users
{
    public static class GetNotFriendUsers
    {
        [FunctionName("GetNotFriendUsers")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/notfriends")]
            HttpRequest request, ILogger logger)
        {
            string username = request.Query["username"];
            List<BsonDocument> userDocuments;
            ObjectResult result;

            if (!ValidationUtils.ValidateUsername(request, logger))
            {
                result = new BadRequestObjectResult("Missing username parameter in query string");
            }
            else if (!await ValidationUtils.CheckIfUserExistsAsync(username))
            {
                result = new BadRequestObjectResult("User does not exist");
            }
            else
            {
                userDocuments = await getAllUsersWhichAreNotFriendsOfUser(username, logger);

                result = new OkObjectResult(ConversionUtils.ConvertBsonDocumentListToJson(userDocuments));
            }

            return result;
        }
        
        private static async Task<List<BsonDocument>> getAllUsersWhichAreNotFriendsOfUser(string username, ILogger logger)
        {
            List<string> friendsUsernamesList;
            List<string> friendRequestsUsernamesList;
            List<string> usersToExclude;
            List<BsonDocument> userDocuments;

            logger.LogInformation($"Getting all users which are friends of user {username}");
            friendsUsernamesList = await getUsersFriendsUsername(username.ToLower());
            logger.LogInformation($"Got all {friendsUsernamesList.Count} friends of user {username}");
            
            logger.LogInformation($"Getting all users which {username} has sent them a friend request");
            friendRequestsUsernamesList = await getUsersFromSentFriendRequests(username.ToLower());
            logger.LogInformation($"Got all {friendRequestsUsernamesList.Count} users which {username} has sent them a friend request");
            
            usersToExclude = friendsUsernamesList.Union(friendRequestsUsernamesList).ToList();
            
            logger.LogInformation($"Getting all users which are not friends of user {username} and which {username} has not sent them a friend request");
            userDocuments = await getAllOtherUsers(usersToExclude);
            logger.LogInformation($"Got all {userDocuments.Count} users which are not friends of user {username} and which {username} has not sent them a friend request");
            
            return userDocuments;
        }

        private static async Task<List<string>> getUsersFriendsUsername(string lowerCasedUsername)
        {
            IMongoCollection<BsonDocument> friendshipCollection;
            FilterDefinition<BsonDocument> friendshipFilter;
            List<BsonDocument> friendshipDocuments;
            List<string> friendsUsernamesList;

            friendshipCollection = AzureDatabaseClient.Instance
                .GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA,
                    collectionName: Constants.COLLECTION_FRIEND);

            friendshipFilter = Builders<BsonDocument>.Filter
                .Where(doc =>  
                    doc["userName1"].AsString.ToLower().Equals(lowerCasedUsername) || 
                    doc["userName2"].AsString.ToLower().Equals(lowerCasedUsername));

            friendshipDocuments = await friendshipCollection
                .Find(friendshipFilter)
                .ToListAsync();
            friendsUsernamesList = friendshipDocuments
                .SelectMany(doc => new[] { doc["userName1"].ToString(), doc["userName2"].ToString() })
                .Distinct()
                .ToList();

            return friendsUsernamesList;
        }
        
        private static async Task<List<string>> getUsersFromSentFriendRequests(string lowerCasedUsername)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);
            FilterDefinition<BsonDocument> userFilter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Regex("userName", new BsonRegularExpression($"^{Regex.Escape(lowerCasedUsername)}$", "i")),
                Builders<BsonDocument>.Filter.Regex("requester", new BsonRegularExpression($"^{Regex.Escape(lowerCasedUsername)}$", "i")));
            List<BsonDocument> friendRequestsDocuments = (await friendRequestsCollection.FindAsync(userFilter)).ToList();
            
            return friendRequestsDocuments.SelectMany(doc => new[] { doc["userName"].ToString(), doc["requester"].ToString() })
                .Distinct()
                .ToList();
        }

        private static async Task<List<BsonDocument>> getAllOtherUsers(List<string> friendsUsernames)
        {
            IMongoCollection<BsonDocument> userCollection = MongoUtils.GetCollection(Constants.COLLECTION_USER);
            FilterDefinition<BsonDocument> userFilter = Builders<BsonDocument>.Filter
                .Where(doc => !friendsUsernames.Contains(doc["userName"].AsString));
            
            return await userCollection
                .Find(userFilter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id").Exclude("password"))
                .ToListAsync();
        }
    }
}
