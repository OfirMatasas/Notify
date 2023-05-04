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
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "friends")]
            HttpRequest req, ILogger log)
        {
            string userId, response;
            List<BsonDocument> friendshipDocuments;
            List<string> friendUsernames;
            ObjectResult result;

            if (string.IsNullOrEmpty(req.Query["username"]))
            {
                log.LogError("The 'userId' query parameter is required");
                result = new BadRequestObjectResult("The 'userId' query parameter is required.");
            }
            else
            {
                userId = req.Query["username"].ToString().ToLower();
                log.LogInformation($"Got client's HTTP request to get friends of user {userId}");

                friendshipDocuments = await GetFriendshipOfSelectedUsernameDocuments(userId);
                friendUsernames = GetFriendUsernames(friendshipDocuments, userId);

                if (friendUsernames.Count.Equals(0))
                {
                    log.LogInformation($"No friends found for user {userId}");
                    result = new NotFoundObjectResult($"No friends found for user {userId}");
                }
                else
                {
                    response = await getAllFriendsOfUser(userId);
                    log.LogInformation($"Retrieved {friendUsernames.Count} friends of user {userId}");
                    result = new OkObjectResult(response);
                }
            }
            
            return result;
        }

        private static async Task<string> getAllFriendsOfUser(string userId)
        {
            IMongoCollection<BsonDocument> userCollection;
            FilterDefinition<BsonDocument> userFilter;
            List<BsonDocument> userDocuments;
            string response;
            
            userCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                databaseName: Constants.DATABASE_NOTIFY_MTA, 
                collectionName: Constants.COLLECTION_USER);
            userFilter = Builders<BsonDocument>.Filter
                .Where(doc => !doc["userName"].ToString().ToLower().Equals(userId));
            userDocuments = await userCollection.Find(userFilter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id").Exclude("password")).ToListAsync();
            response = Utils.ConversionUtils.ConvertBsonDocumentListToJson(userDocuments);
            
            return response;
        }

        private static async Task<List<BsonDocument>> GetFriendshipOfSelectedUsernameDocuments(string userId)
        {
            IMongoCollection<BsonDocument> friendshipCollection = AzureDatabaseClient.Instance
                .GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA, 
                    collectionName: Constants.COLLECTION_FRIEND);
            FilterDefinition<BsonDocument> friendshipFilter = Builders<BsonDocument>.Filter
                .Where(doc => doc["userName1"].ToString().ToLower().Equals(userId) || 
                              doc["userName2"].ToString().ToLower().Equals(userId));
            List<BsonDocument> friendshipsList = await friendshipCollection.Find(friendshipFilter).ToListAsync();
            
            return friendshipsList;
        }

        private static List<string> GetFriendUsernames(List<BsonDocument> friendshipDocuments, string userId)
        {
            List<string> friendsUsernamesList = friendshipDocuments
                .SelectMany(doc => new[] { doc["userName1"].ToString().ToLower(), doc["userName2"].ToString().ToLower() })
                .Distinct()
                .Where(username => username != userId)
                .ToList();

            return friendsUsernamesList;
        }
    }
}
