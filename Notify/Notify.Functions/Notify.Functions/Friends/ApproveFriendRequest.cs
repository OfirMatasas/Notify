using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;

namespace Notify.Functions.Friends
{
    public static class ApproveFriendRequest
    {
        [FunctionName("ApproveFriendRequest")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "friend/approve")]
            HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string requester, username;
            ObjectResult result = null;

            try
            {
                requester = Convert.ToString(data.requester);
                username = Convert.ToString(data.userName);
                
                log.LogInformation($"Approving friend request from {requester} to {username}");
                await createFriendship(requester, username, log);
                await deleteFriendRequest(requester, username, log);

                log.LogInformation($"Friendship approved in the DB. requester: {requester}, username: {username}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error approving friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return (IActionResult)result ?? new OkResult();
        }

        private static async Task createFriendship(string requester, string username, ILogger log)
        {
            IMongoCollection<BsonDocument> friendsCollection;
            BsonDocument friendDocument;

            friendsCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                Constants.DATABASE_NOTIFY_MTA, 
                Constants.COLLECTION_FRIEND);

            friendDocument = new BsonDocument
                {
                    { "userName1", requester },
                    { "userName2", username }
                };

            log.LogInformation($"Creating friendship document between {requester} and {username}");
            
            await friendsCollection.InsertOneAsync(friendDocument);

            log.LogInformation($"Friendship document between {requester} and {username} created successfully");
        }

        private static async Task deleteFriendRequest(string requester, string username, ILogger log)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            FilterDefinition<BsonDocument> friendRequestsFilter;

            friendRequestsCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                Constants.DATABASE_NOTIFY_MTA, 
                Constants.COLLECTION_FRIEND_REQUEST);

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

