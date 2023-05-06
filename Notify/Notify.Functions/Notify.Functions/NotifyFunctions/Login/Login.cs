using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

namespace Notify.Functions.NotifyFunctions.Login;

public static class Login
{
    [FunctionName("Login")]
    [AllowAnonymous]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req, ILogger log)
    {
        IMongoCollection<BsonDocument> collection;
        string requestBody;
        dynamic data;
        ObjectResult result;
        
        log.LogInformation("Got client's HTTP request to login");

        try
        {
            collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA,
                    Constants.COLLECTION_USER);
            log.LogInformation(
                $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");

            requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation($"Data:{Environment.NewLine}{data}");

            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("userName", data.userName.ToString()),
                Builders<BsonDocument>.Filter.Eq("password", data.password.ToString())
            );
            
            List<BsonDocument> documents = collection.Find(filter).ToList();
            if (documents.Count == 0)
            {
                log.LogInformation($"No user found with username {data.userName} and password {data.password}");
                result = new NotFoundObjectResult($"No user found with username {data.userName} and password {data.password}");
            }
            if (documents.Count > 1)
            {
                log.LogInformation(
                    $"More than one user found with username {data.userName} and password {data.password}");
                result = new ConflictObjectResult($"No user found with username {data.userName} and password {data.password}");
            }

            log.LogInformation($"Found user with username {data.userName} and password {data.password}");
            result = new OkObjectResult(requestBody);
        }
        catch (Exception e)
        {
            log.LogError(e.Message);
            result = new BadRequestObjectResult($"Failed to login. Error: {e.Message}");
        }

        return result;
    }
}
