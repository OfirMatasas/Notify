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
    public static class RejectFriendRequest
    {
        [FunctionName("RejectFriendRequest")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "friend/reject")]
            HttpRequest request, ILogger logger)
        {
            dynamic data;
            string requester, username;
            ObjectResult result = null;

            try
            {
                data = await ConversionUtils.ExtractBodyContentAsync(request);
                requester = Convert.ToString(data.requester);
                username = Convert.ToString(data.userName);
                
                logger.LogInformation($"Rejecting friend request from {requester} to {username}");
                await deletePendingFriendRequest(requester, username);

                logger.LogInformation($"Friend request rejected. requester: {requester}, username: {username}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error rejecting friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return result;
        }

        private static async Task deletePendingFriendRequest(string requester, string username)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            FilterDefinition<BsonDocument> friendRequestsFilter;

            friendRequestsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);

            friendRequestsFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("requester", requester),
                Builders<BsonDocument>.Filter.Eq("userName", username)
            );

            await friendRequestsCollection.DeleteOneAsync(friendRequestsFilter);
        }
    }
}
