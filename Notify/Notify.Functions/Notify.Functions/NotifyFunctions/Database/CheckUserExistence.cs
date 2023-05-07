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

namespace Notify.Functions.NotifyFunctions.Database;

public static class CheckUserExistence
{
    [FunctionName("CheckUserExistence")]
    [AllowAnonymous]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkUserExistence")]
        HttpRequest req, ILogger log)
    {
        IMongoCollection<BsonDocument> collection;
        string requestBody;
        dynamic data;
        ObjectResult result;

        log.LogInformation("Got client's HTTP request to check user existence");

        try
        {
            collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA,
                Constants.COLLECTION_USER);
            log.LogInformation(
                $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");

            requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation($"Data:{Environment.NewLine}{data}");

            result = await handleUserExistenceCheck(data, collection, log);
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message);
            result = new BadRequestObjectResult($"Failed to check user existence. Error: {ex.Message}");
        }

        return result;
    }

    private static async Task<IActionResult> handleUserExistenceCheck(dynamic data,
        IMongoCollection<BsonDocument> collection, ILogger log)
    {
        ObjectResult result;

        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userName", Convert.ToString(data.userName)) |
                         Builders<BsonDocument>.Filter.Eq("telephone", Convert.ToString(data.telephone));
            var count = await collection.CountDocumentsAsync(filter);
            if (count > 0)
            {
                log.LogInformation($"User with username {data.userName} or telephone {data.telephone} already exists");

                result = new ConflictObjectResult($"User with username {data.userName} or telephone {data.telephone} already exists");
            }
            else
            {
                log.LogInformation($"User with username {data.userName} and telephone {data.telephone} does not exist");

                result = new OkObjectResult(JsonConvert.SerializeObject(data));
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Failed to check user existence. Reason: {ex.Message}");

            result = new BadRequestObjectResult($"Failed to check user existence. Error: {ex.Message}");
        }
        
        return result;
    }
}
