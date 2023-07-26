using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;
using Constants = Notify.Functions.Core.Constants;

namespace Notify.Functions.Friends
{
    public static class GetFriends
    {
        [FunctionName("GetFriends")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "friend")]
            HttpRequest req, ILogger log)
        {
            string lowerCasedUsername, response;
            List<BsonDocument> friendshipDocuments;
            List<string> friendUsernames;
            ObjectResult result;

            if (string.IsNullOrEmpty(req.Query["username"]))
            {
                log.LogError("The 'username' query parameter is required");
                result = new BadRequestObjectResult("The 'username' query parameter is required.");
            }
            else
            {
                lowerCasedUsername = req.Query["username"].ToString().ToLower();
                log.LogInformation($"Got client's HTTP request to get friends of user {lowerCasedUsername}");

                friendshipDocuments = await GetFriendshipOfSelectedUsernameDocuments(lowerCasedUsername);
                friendUsernames = GetFriendUsernames(friendshipDocuments, lowerCasedUsername);

                if (friendUsernames.Count.Equals(0))
                {
                    log.LogInformation($"No friends found for user {lowerCasedUsername}");
                    result = new NotFoundObjectResult($"No friends found for user {lowerCasedUsername}");
                }
                else
                {
                    response = await getAllFriendsOfUser(friendUsernames);
                    log.LogInformation($"Retrieved {friendUsernames.Count} friends of user {lowerCasedUsername}:");
                    log.LogInformation(string.Join(Environment.NewLine, friendUsernames.Select(username => $"- {username}")));
                    log.LogInformation(response);
                    result = new OkObjectResult(response);
                }
            }
            
            return result;
        }
        
        private static async Task<List<BsonDocument>> GetFriendshipOfSelectedUsernameDocuments(string lowerCasedUsername)
        {
            IMongoCollection<BsonDocument> friendshipCollection;
            FilterDefinition<BsonDocument> friendshipFilter;
            List<BsonDocument> friendshipsList;

            friendshipCollection = AzureDatabaseClient.Instance
                .GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA, 
                    collectionName: Constants.COLLECTION_FRIEND);

            friendshipFilter = Builders<BsonDocument>.Filter.Regex(
                                   "userName1", 
                                   new BsonRegularExpression(lowerCasedUsername, "i")) |
                               Builders<BsonDocument>.Filter.Regex(
                                   "userName2", 
                                   new BsonRegularExpression(lowerCasedUsername, "i"));

            friendshipsList = await friendshipCollection.Find(friendshipFilter).ToListAsync();

            return friendshipsList;
        }
        
        private static List<string> GetFriendUsernames(List<BsonDocument> friendshipDocuments, string lowerCasedUsername)
        {
            List<string> friendsUsernamesList = friendshipDocuments
                .SelectMany(doc => new[] { doc["userName1"].ToString(), doc["userName2"].ToString() })
                .Distinct()
                .Where(username => !username.ToLower().Equals(lowerCasedUsername))
                .ToList();

            return friendsUsernamesList;
        }

        private static async Task<string> getAllFriendsOfUser(List<string> friendUsernames)
        {
            IMongoCollection<BsonDocument> userCollection;
            FilterDefinition<BsonDocument> userFilter;
            List<BsonDocument> userDocuments;
    
            userCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                databaseName: Constants.DATABASE_NOTIFY_MTA, 
                collectionName: Constants.COLLECTION_USER);
            userFilter = Builders<BsonDocument>.Filter.In("userName", friendUsernames);
            userDocuments = await userCollection.Find(userFilter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id").Exclude("password"))
                .ToListAsync();
            
            return Utils.ConversionUtils.ConvertBsonDocumentListToJson(userDocuments);
        }
    }
}
