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

namespace Notify.Functions.NotifyFunctions.Login
{
    public static class Login
    {
        [FunctionName("Login")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            FilterDefinition<BsonDocument> filter;
            List<BsonDocument> documents;
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

                filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Regex("userName",
                        new BsonRegularExpression(Convert.ToString(data.userName), "i")),
                    Builders<BsonDocument>.Filter.Eq("password",
                        AzureVault.AzureVault.DecryptPasswordWithKeyVault(data.password.ToString(),
                            Constants.PASSWORD_ENCRYPTION_KEY))
                );

                documents = collection.Find(filter).ToList();
                if (documents.Count.Equals(0))
                {
                    log.LogInformation($"No user found with username {data.userName} and password {data.password}");
                    result = new NotFoundObjectResult("Invalid username or password");
                }
                else if (documents.Count > 1)
                {
                    log.LogInformation(
                        $"More than one user found with username {data.userName} and password {data.password}");
                    result = new ConflictObjectResult("Invalid username or password");
                }
                else
                {
                    log.LogInformation($"Found one user with username {data.userName} and password {data.password}");
                    result = new OkObjectResult(requestBody);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new BadRequestObjectResult($"Failed to login: {ex.Message}");
            }

            return result;
        }
    }
}
