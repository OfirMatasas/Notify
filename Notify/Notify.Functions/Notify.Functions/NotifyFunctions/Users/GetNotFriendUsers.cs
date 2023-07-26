using System;
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

namespace Notify.Functions.NotifyFunctions.Users
{
    public static class GetNotFriendUsers
    {
        [FunctionName("GetNotFriendUsers")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/notfriends")]
            HttpRequest req, ILogger log)
        {
            string username = req.Query["username"];
            List<BsonDocument> userDocuments;
            ObjectResult result;

            if (!ValidationUtils.ValidateUserName(req, log))
            {
                result = new BadRequestObjectResult("Missing username parameter in query string");
            }
            else if (!await usernameExists(username))
            {
                result = new BadRequestObjectResult("User does not exist");
            }
            else
            {
                userDocuments = await getAllUsersWhichAreNotFriendsOfUser(username, log);

                result = new OkObjectResult(ConversionUtils.ConvertBsonDocumentListToJson(userDocuments));
            }

            return result;
        }

        private static async Task<bool> usernameExists(string username)
        {
            IMongoCollection<BsonDocument> collection;
            FilterDefinition<BsonDocument> filterUsername;
            long countUsername;

            collection = AzureDatabaseClient.Instance
                .GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA,
                    collectionName: Constants.COLLECTION_USER);
            filterUsername = Builders<BsonDocument>.Filter.Regex("userName",
                new BsonRegularExpression($"^{Regex.Escape(Convert.ToString(username))}$", "i"));

            countUsername = await collection.CountDocumentsAsync(filterUsername);
            return countUsername > 0;
        }

        private static async Task<List<BsonDocument>> getAllUsersWhichAreNotFriendsOfUser(string username, ILogger log)
        {
            List<string> friendsUsernamesList;
            List<BsonDocument> userDocuments;

            log.LogInformation($"Getting all users which are not friends of user {username}");
            friendsUsernamesList = await getUsersFriendsUsername(username.ToLower());
            log.LogInformation($"Got all {friendsUsernamesList.Count} friends of user {username}");

            log.LogInformation($"Getting all users which are not friends of user {username}");
            userDocuments = await getAllOtherUsers(friendsUsernamesList);
            log.LogInformation($"Got all {userDocuments.Count} users which are not friends of user {username}");
            
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

        private static async Task<List<BsonDocument>> getAllOtherUsers(List<string> friendsUsernames)
        {
            IMongoCollection<BsonDocument> userCollection;
            FilterDefinition<BsonDocument> userFilter;

            userCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                databaseName: Constants.DATABASE_NOTIFY_MTA,
                collectionName: Constants.COLLECTION_USER);
            userFilter = Builders<BsonDocument>.Filter
                .Where(doc => !friendsUsernames.Contains(doc["userName"].AsString));
            
            return await userCollection
                .Find(userFilter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id").Exclude("password"))
                .ToListAsync();
        }
    }
}
