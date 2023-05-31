using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Login
{
    public static class UserRegistration
    {
        [FunctionName("UserRegistration")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            long usersCount;
            ObjectResult result;

            log.LogInformation("Got client's HTTP request to register");

            try
            {
                collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA,
                    Constants.COLLECTION_USER);
                log.LogInformation(
                    $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");

                await MongoUtils.CreatePropertyIndexes(collection, "userName", "telephone");

                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
                log.LogInformation($"Data:{Environment.NewLine}{data}");

                filter = Builders<BsonDocument>.Filter.Regex("userName",
                    new BsonRegularExpression(Convert.ToString(data.userName), "i"));

                usersCount = await collection.CountDocumentsAsync(filter);
                if (usersCount > 0)
                {
                    log.LogInformation($"Username '{data.userName}' already exists");
                    result = new ConflictObjectResult($"Username '{data.userName}' already exists");
                }
                else
                {
                    data.password =
                        await AzureVault.AzureVault.ProcessPasswordWithKeyVault(Convert.ToString(data.password),
                            Constants.PASSWORD_ENCRYPTION_KEY, "encrypt");

                    BsonDocument userDocument = new BsonDocument
                    {
                        { "name", Convert.ToString(data.name) },
                        { "userName", Convert.ToString(data.userName) },
                        { "password", Convert.ToString(data.password) },
                        { "telephone", Convert.ToString(data.telephone) }
                    };

                    await collection.InsertOneAsync(userDocument);
                    log.LogInformation(
                        $"Inserted user with username {data.userName} and telephone {data.telephone} into database");

                    result = new OkObjectResult(JsonConvert.SerializeObject(data));
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to insert user. Reason: {ex.Message}");
                result = new ObjectResult($"Failed to register.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }
    }
}
