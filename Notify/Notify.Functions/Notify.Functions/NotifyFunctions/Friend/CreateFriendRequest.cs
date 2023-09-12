using System;
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
using Notify.Functions.Core;
using Notify.Functions.Utils;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Friend
{
    public static class CreateFriendRequest
    {
        [FunctionName("CreateFriendRequest")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "friend/request")]
            HttpRequest request, ILogger logger)
        {
            dynamic data;
            string requester, username, requestDate;
            ObjectResult result;

            try
            {
                data = await ConversionUtils.ExtractBodyContentAsync(request);
                requester = Convert.ToString(data.requester);
                username = Convert.ToString(data.userName);
                requestDate = Convert.ToString(data.requestDate);

                result = friendRequestShouldBeCreated(requester, username, logger);

                if (result is null)
                {
                    throw new Exception("Unexpected error occurred");
                }

                if (result is OkObjectResult)
                {
                    logger.LogInformation($"Creating friend request from {requester} to {username}");
                    await createFriendRequest(requester, username, requestDate, logger);

                    logger.LogInformation(
                        $"$Friend request created. requester: {requester}, username: {username}, requestDate: {requestDate}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error creating friend request: {ex.Message}");
                result = new ExceptionResult(ex, false);
            }

            return result;
        }

        private static ObjectResult friendRequestShouldBeCreated(string requester, string username, ILogger logger)
        {
            ObjectResult result;

            if (string.IsNullOrEmpty(requester) || string.IsNullOrEmpty(username))
            {
                logger.LogInformation($"Missing requester or username parameter in request body");
                result = new BadRequestObjectResult("Missing requester or username parameter in request body");
            }
            else if (requester.Equals(username))
            {
                logger.LogInformation($"Requester and username cannot be the same");
                result = new BadRequestObjectResult("Requester and username cannot be the same");
            }
            else if (!usersExist(requester, username))
            {
                logger.LogInformation($"Requester or username does not exist");
                result = new BadRequestObjectResult("Requester or username does not exist");
            }
            else if (bothUsersAreAlreadyFriends(requester, username))
            {
                logger.LogInformation($"Requester and username are already friends");
                result = new BadRequestObjectResult("Requester and username are already friends");
            }
            else if (friendRequestAlreadyExists(requester, username))
            {
                logger.LogInformation($"Friend request already exists");
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

            friendRequestsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);

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

            friendsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND);

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
            return documentsFoundCount > 0;
        }

        private static bool usersExist(string requester, string username)
        {
            IMongoCollection<BsonDocument> usersCollection;
            FilterDefinition<BsonDocument> usersFilter;
            long numberOfUsersFound;

            usersCollection = MongoUtils.GetCollection(Constants.COLLECTION_USER);
            usersFilter = Builders<BsonDocument>.Filter.Regex("userName",
                              new BsonRegularExpression($"^{Regex.Escape(username)}$", "i")) |
                          Builders<BsonDocument>.Filter.Regex("userName",
                              new BsonRegularExpression($"^{Regex.Escape(requester)}$", "i"));

            numberOfUsersFound = usersCollection.CountDocuments(usersFilter);
            return numberOfUsersFound.Equals(2);
        }

        private static async Task createFriendRequest(string requester, string username, string requestDate, ILogger logger)
        {
            IMongoCollection<BsonDocument> friendRequestsCollection;
            BsonDocument friendRequestDocument;

            friendRequestsCollection = MongoUtils.GetCollection(Constants.COLLECTION_FRIEND_REQUEST);

            friendRequestDocument = new BsonDocument
            {
                { "requester", requester },
                { "userName", username },
                { "requestDate", requestDate },
            };

            await friendRequestsCollection.InsertOneAsync(friendRequestDocument);
            logger.LogInformation($"Friend request from {requester} to {username} created successfully");

            await createNewsfeedForUser(username, requester, logger);
        }

        private static async Task createNewsfeedForUser(string username, string requester, ILogger logger)
        {
            IMongoCollection<BsonDocument> newsfeedCollection = MongoUtils.GetCollection(Constants.COLLECTION_NEWSFEED);
            BsonDocument newsfeedDocument = new BsonDocument
            {
                { "username", username },
                { "title", "New friend request" },
                { "content", $"You have a new friend request from {requester}" }
            };

            await newsfeedCollection.InsertOneAsync(newsfeedDocument);
            logger.LogInformation($"News created for user {username} about new friend request from {requester}");
        }
    }
}
