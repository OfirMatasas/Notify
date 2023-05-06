using System;
using System.Collections.Generic;
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

namespace Notify.Functions.NotifyFunctions.Database;

public static class RegisterUser
{
    [FunctionName("RegisterUser")]
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

            await Utils.MongoUtils.CreatePropertyIndex(collection, "userName");
            
            result = await handleUserRegistration(data, collection, log);
            
        }
        catch (Exception e)
        {
            log.LogError(e.Message);
            result = new BadRequestObjectResult($"Failed to register. Error: {e.Message}");
        }

        return result;
    }
    
    private static async Task<IActionResult> handleUserRegistration(dynamic data,
        IMongoCollection<BsonDocument> collection, ILogger log)
    {
        BsonDocument userDocument = new BsonDocument
        {
            { "name", Convert.ToString(data.name) },
            { "userName", Convert.ToString(data.userName) },
            { "password", Convert.ToString(data.password) },
            { "telephone", Convert.ToString(data.telephone) }
        };
        
        try
        {
            CreateIndexOptions indexOptions = new CreateIndexOptions
            {
                Unique = true
            };
            IndexKeysDefinition<BsonDocument> indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("userName");

            await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
        }
        catch (MongoCommandException ex)
        {
            log.LogWarning($"Failed to create unique index. Reason: {ex.Message}");
        }

        try
        {
            await collection.InsertOneAsync(userDocument);
            log.LogInformation($"Inserted user with username {data.userName} into database");

            return new OkObjectResult(JsonConvert.SerializeObject(data));
        }
        catch (MongoWriteException ex)
        {
            log.LogError($"Failed to insert user. Reason: {ex.Message}");

            return new ConflictObjectResult($"User with username {data.userName} already exists");
        }
    }

}

