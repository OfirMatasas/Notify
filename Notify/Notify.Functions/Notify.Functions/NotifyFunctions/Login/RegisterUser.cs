using System;
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
using Notify.Functions.Utils;
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
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            long usersCount;
            ObjectResult result;

            log.LogInformation("Got client's HTTP request to register");

            try
            {
                collection = MongoUtils.GetCollection(Constants.COLLECTION_USER);
                await MongoUtils.CreatePropertyIndexesAsync(collection, log, "userName", "telephone");
                
                data = await ConversionUtils.ExtractBodyContentAsync(req);
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
                        { "telephone", Convert.ToString(data.telephone) },
                        { "profilePicture", Constants.BLOB_DEFAULT_PROFILE_IMAGE }
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
