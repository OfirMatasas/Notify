using System.Collections.Generic;
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
using Notify.Functions.HTTPClients;
using Notify.Functions.Utils;

namespace Notify.Functions.NotifyFunctions.FriendRequest
{
    public static class GetFriendRequests
    {
        [FunctionName("GetFriendRequests")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "friend/request")]
            HttpRequest request, ILogger logger)
        {
            string response, username;
            List<BsonDocument> friendRequestDocuments;
            ObjectResult result;

            username = request.Query["username"];

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
                logger.LogInformation(
                    $"Got client's HTTP request to list all pending friend requests of user {username}");

                friendRequestDocuments = await GetPendingFriendRequestOfSelectedUsernameDocuments(username);

                if (friendRequestDocuments.Count.Equals(0))
                {
                    logger.LogInformation($"No pending friend requests found for user {username}");
                    result = new NotFoundObjectResult($"No pending friend requests found for user {username}");
                }
                else
                {
                    logger.LogInformation("Getting profile pictures of users who sent friend requests");
                    await GetProfilePicturesOfUsersAsync(friendRequestDocuments, logger);
                    
                    response = ConversionUtils.ConvertBsonDocumentListToJson(friendRequestDocuments);
                    logger.LogInformation(
                        $"Retrieved {friendRequestDocuments.Count} pending friend requests of user {username}:");
                    logger.LogInformation(response);
                    result = new OkObjectResult(response);
                }
            }

            return result;
        }

        private static async Task<List<BsonDocument>> GetPendingFriendRequestOfSelectedUsernameDocuments(string username)
        {
            IMongoCollection<BsonDocument> pendingFriendRequestsCollection;
            FilterDefinition<BsonDocument> pendingFriendRequestFilter;
            List<BsonDocument> pendingFriendRequestsList;

            pendingFriendRequestsCollection = AzureDatabaseClient.Instance
                .GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA,
                    collectionName: Constants.COLLECTION_FRIEND_REQUEST);

            pendingFriendRequestFilter = Builders<BsonDocument>.Filter.Regex(
                "userName", new BsonRegularExpression($"^{Regex.Escape(username)}$", "i"));

            pendingFriendRequestsList = await pendingFriendRequestsCollection
                .Find(pendingFriendRequestFilter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .ToListAsync();

            return pendingFriendRequestsList;
        }
        
        private static async Task GetProfilePicturesOfUsersAsync(List<BsonDocument> requestDocuments, ILogger logger)
        {
            IMongoCollection<BsonDocument> usersCollection = Utils.MongoUtils.GetCollection(Constants.COLLECTION_USER);
            FilterDefinition<BsonDocument> userFilter;
            BsonDocument userDocument;

            foreach (BsonDocument requestDocument in requestDocuments)
            {
                userFilter = Builders<BsonDocument>.Filter.Where(
                    document => document["userName"].AsString.Equals(requestDocument["requester"].AsString));
                
                userDocument = usersCollection.Find(userFilter).FirstOrDefault();
                
                if(userDocument != null)
                {
                    if (userDocument.Contains("profilePicture"))
                    {
                        requestDocument["profilePicture"] = userDocument["profilePicture"].AsString;
                    }
                    else
                    {
                        requestDocument["profilePicture"] = "";
                    }
                }
                else
                {
                    logger.LogWarning($"User {requestDocument["requester"].AsString} not found");
                    requestDocument["profilePicture"] = "to_be_removed";
                }
            }
            
            requestDocuments.RemoveAll(doc => doc["profilePicture"].AsString.Equals("to_be_removed"));
        }
    }
}
