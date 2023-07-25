using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;

namespace Notify.Functions.Friends
{
    public static class CreateFriendRequest
    {
        [FunctionName("CreateFriendRequest")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "friends/request")]
            HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string requester, username, requestDate, status;
            ObjectResult result;

            try
            {
                requester = Convert.ToString(data.requester);
                username = Convert.ToString(data.userName);
                requestDate = Convert.ToString(data.requestDate);
                status = Convert.ToString(data.status);
                result = friendRequestShouldBeCreated(requester, username, log);
                
                if (result is null)
                {
                    throw new Exception("Unexpected error occurred");
                }
                
                if (result is OkObjectResult)
                {
                    log.LogInformation($"Creating friend request from {requester} to {username}");
                    await createFriendRequest(requester, username, requestDate, status);

                    log.LogInformation($"$Friend request created. requester: {requester}, username: {username}, requestDate: {requestDate}, status: {status}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error creating friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return result;
        }
        
        private static ObjectResult friendRequestShouldBeCreated(string requester, string username, ILogger log)
        {
            ObjectResult result;
            
            if (string.IsNullOrEmpty(requester) || string.IsNullOrEmpty(username))
            {
                log.LogInformation($"Missing requester or username parameter in request body");
                result = new BadRequestObjectResult("Missing requester or username parameter in request body");
            }
            else if(requester.Equals(username))
            {
                log.LogInformation($"Requester and username cannot be the same");
                result = new BadRequestObjectResult("Requester and username cannot be the same");
            }
            else if(!usersExist(requester, username))
            {
                log.LogInformation($"Requester or username does not exist");
                result = new BadRequestObjectResult("Requester or username does not exist");
            }
            else if(bothUsersAreAlreadyFriends(requester, username))
            {
                log.LogInformation($"Requester and username are already friends");
                result = new BadRequestObjectResult("Requester and username are already friends");
            }
            else if(friendRequestAlreadyExists(requester, username))
            {
                log.LogInformation($"Friend request already exists");
                result = new BadRequestObjectResult("Friend request already exists");
            }
            else
            {
                result = new OkObjectResult("Friend request created");
            }

            return result;
        }

        private static bool friendRequestAlreadyExists(string requester, string username)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            FilterDefinition<BsonDocument> friendRequestsFilter;
            long documentsFoundCount;

            friendRequestsCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                Constants.DATABASE_NOTIFY_MTA, 
                Constants.COLLECTION_FRIEND_REQUEST);

            friendRequestsFilter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Regex("requester",
                        new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("userName",
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i"))
                ),
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Regex("requester",
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("userName",
                        new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i"))
                )
            );            
            documentsFoundCount = friendRequestsCollection.CountDocuments(friendRequestsFilter);
            return !documentsFoundCount.Equals(0);
        }

        private static bool bothUsersAreAlreadyFriends(string requester, string username)
        {
            IMongoCollection<BsonDocument> friendsCollection;
            FilterDefinition<BsonDocument> friendsFilter;
            long documentsFoundCount;

            friendsCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                Constants.DATABASE_NOTIFY_MTA, 
                Constants.COLLECTION_FRIEND);

            friendsFilter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Regex("userName1",
                        new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("userName2",
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i"))
                ),
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Regex("userName1",
                        new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")),
                    Builders<BsonDocument>.Filter.Regex("userName2",
                        new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i"))
                )
            );
            
            documentsFoundCount = friendsCollection.CountDocuments(friendsFilter);
            return !documentsFoundCount.Equals(0);
        }

        private static bool usersExist(string requester, string username)
        {
            IMongoCollection<BsonDocument> usersCollection;
            FilterDefinition<BsonDocument> usersFilter;
            long numberOfUsersFound;

            usersCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                Constants.DATABASE_NOTIFY_MTA, 
                Constants.COLLECTION_USER);
            usersFilter = Builders<BsonDocument>.Filter.Regex("userName", 
                              new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")) | 
                          Builders<BsonDocument>.Filter.Regex("userName", 
                              new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i"));

            numberOfUsersFound = usersCollection.CountDocuments(usersFilter);
            return numberOfUsersFound.Equals(2);
        }
        
        private static async Task createFriendRequest(string requester, string username, string requestDate, string status)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            BsonDocument friendRequestDocument;

            friendRequestsCollection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(
                Constants.DATABASE_NOTIFY_MTA, 
                Constants.COLLECTION_FRIEND_REQUEST);

            friendRequestDocument = new BsonDocument
                {
                    { "requester", requester },
                    { "userName", username },
                    { "requestDate", requestDate },
                    { "status", status }
                };
            
            await friendRequestsCollection.InsertOneAsync(friendRequestDocument);
        }
    }
}
