using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Database
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
            ObjectResult result;

            log.LogInformation("Got client's HTTP request to register");

            try
            {
                collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA,
                    Constants.COLLECTION_USER);
                log.LogInformation(
                    $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");

                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
                log.LogInformation($"Data:{Environment.NewLine}{data}");

                await MongoUtils.CreatePropertyIndexes(collection, "userName", "telephone");

                result = await HandleUserRegistrationAsync(data, collection, log);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new BadRequestObjectResult($"Failed to register. Error: {ex.Message}");
            }

            return result;
        }

        private static async Task<IActionResult> HandleUserRegistrationAsync(dynamic data,
            IMongoCollection<BsonDocument> collection, ILogger log)
        {
            ObjectResult result;

            var filter = Builders<BsonDocument>.Filter.Eq("userName", Convert.ToString(data.userName)) |
                         Builders<BsonDocument>.Filter.Eq("telephone", Convert.ToString(data.telephone));
            var count = await collection.CountDocumentsAsync(filter);
            if (count > 0)
            {
                result = new ConflictObjectResult($"User with {filter.RenderToJson()} already exists.");
                
                return result;
            }

            BsonDocument userDocument = new BsonDocument
            {
                { "name", Convert.ToString(data.name) },
                { "userName", Convert.ToString(data.userName) },
                { "password", Convert.ToString(data.password) },
                { "telephone", Convert.ToString(data.telephone) }
            };

            try
            {
                await collection.InsertOneAsync(userDocument);
                log.LogInformation(
                    $"Inserted user with username {data.userName} and telephone {data.telephone} into database");

                result = new OkObjectResult(JsonConvert.SerializeObject(data));
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                string duplicateField = ex.WriteError.Message.Split('\'')[1];
                log.LogError($"Failed to insert user with duplicate {duplicateField}. Reason: {ex.Message}");

                result = new ConflictObjectResult($"User with {duplicateField} '{data[duplicateField]}' already exists");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to insert user. Reason: {ex.Message}");

                result = new BadRequestObjectResult($"Failed to register. Error: {ex.Message}");
            }
            
            return result;
        }
    }
}

