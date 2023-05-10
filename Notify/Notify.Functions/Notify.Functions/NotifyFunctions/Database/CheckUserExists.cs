using System;
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
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Database
{
    public static class CheckUserExists
    {
        [FunctionName("CheckUserExists")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkUserExists")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            ObjectResult result;
            dynamic filterUsername;
            dynamic filterTelephone;
            dynamic countUsername;
            dynamic countTelephone;

            log.LogInformation("Got client's HTTP request to check if user exists");

            try
            {
                collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA,
                    Constants.COLLECTION_USER);
                log.LogInformation(
                    $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");

                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
                log.LogInformation($"Data:{Environment.NewLine}{data}");
                
                filterUsername = Builders<BsonDocument>.Filter.Eq("userName", Convert.ToString(data.username));
                filterTelephone = Builders<BsonDocument>.Filter.Eq("telephone", Convert.ToString(data.telephone));

                countUsername = await collection.CountDocumentsAsync(filterUsername);
                countTelephone = await collection.CountDocumentsAsync(filterTelephone);

                if (countUsername > 0 && countTelephone > 0)
                {
                    result = new ConflictObjectResult(
                        $"User with username '{data.username}' and telephone '{data.telephone}' already exists.");
                }
                else if (countUsername > 0)
                {
                    result = new ConflictObjectResult($"User with username '{data.username}' already exists.");
                }
                else if (countTelephone > 0)
                {
                    result = new ConflictObjectResult($"User with telephone '{data.telephone}' already exists.");
                }
                else
                {
                    result = new OkObjectResult(JsonConvert.SerializeObject(data));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new BadRequestObjectResult($"Failed to check if user exists. Error: {ex.Message}");
            }

            return result;
        }
    }
}