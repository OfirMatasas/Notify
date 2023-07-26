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

namespace Notify.Functions.Friends;

namespace Notify.Functions.Friends
{
    public static class RejectFriendRequest
    {
        [FunctionName("RejectFriendRequest")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "friend/reject")]
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
                
                log.LogInformation($"Rejecting friend request from {requester} to {username}");
                await deleteFriendRequest(requester, username);

                log.LogInformation($"Friend request rejected. requester: {requester}, username: {username}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error rejecting friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return result;
        }

        private static async Task deleteFriendRequest(string requester, string username)
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

            await friendRequestsCollection.DeleteOneAsync(friendRequestsFilter);
        }
    }
}
