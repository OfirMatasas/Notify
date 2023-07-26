using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
using Notify.Functions.Utils;

namespace Notify.Functions.Friends;

public static class GetPendingFriendRequests
{
    [FunctionName("GetPendingFriendRequests")]
    [AllowAnonymous]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "friend/request")]
        HttpRequest req, ILogger log)
    {
        string lowerCasedUsername, response, username;
        List<BsonDocument> friendRequestDocuments;
        ObjectResult result;

        username = req.Query["username"];

        if (!ValidationUtils.ValidateUserName(req, log))
        {
            return new BadRequestObjectResult("Missing username parameter in query string");
        }

        if (!await ValidationUtils.DoesUsernameExist(username))
        {
            return new BadRequestObjectResult("User does not exist");
        }

        lowerCasedUsername = username.ToLower();

        log.LogInformation(
            $"Got client's HTTP request to list all pending friend requests of user {lowerCasedUsername}");

        friendRequestDocuments = await GetPendingFriendRequestOfSelectedUsernameDocuments(lowerCasedUsername);

        if (friendRequestDocuments.Count.Equals(0))
        {
            log.LogInformation($"No pending friend requests found for user {lowerCasedUsername}");
            result = new NotFoundObjectResult($"No pending friend requests found for user {lowerCasedUsername}");
        }
        else
        {
            response = Utils.ConversionUtils.ConvertBsonDocumentListToJson(friendRequestDocuments);
            log.LogInformation(
                $"Retrieved {friendRequestDocuments.Count} pending friend requests of user {lowerCasedUsername}:");
            log.LogInformation(response);
            result = new OkObjectResult(response);
        }

        return result;
    }

    private static async Task<List<BsonDocument>> GetPendingFriendRequestOfSelectedUsernameDocuments(string lowerCasedUsername)
    {
        IMongoCollection<BsonDocument> pendingFriendRequestsCollection;
        FilterDefinition<BsonDocument> pendingFriendRequestFilter;
        List<BsonDocument> pendingFriendRequestsList;

        pendingFriendRequestsCollection = AzureDatabaseClient.Instance
            .GetCollection<BsonDocument>(
                databaseName: Constants.DATABASE_NOTIFY_MTA, 
                collectionName: Constants.COLLECTION_FRIEND_REQUEST);

        pendingFriendRequestFilter = Builders<BsonDocument>.Filter.Eq(
            "userName", lowerCasedUsername);

        pendingFriendRequestsList = await pendingFriendRequestsCollection
            .Find(pendingFriendRequestFilter)
            .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
            .ToListAsync();

        return pendingFriendRequestsList;
    }
}