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
            string decryptedPassword;
            string storedEncryptedPassword;
            BsonDocument user;

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
                        new BsonRegularExpression(Convert.ToString(data.userName), "i"))
                );

                user = await collection.Find(filter).FirstOrDefaultAsync();

                if (user.IsBsonNull)
                {
                    log.LogInformation($"No user found with username {data.userName}");
                    result = new NotFoundObjectResult("Invalid username or password");
                }
                else
                {
                    storedEncryptedPassword = user.GetValue("password").ToString();
                    decryptedPassword = await AzureVault.AzureVault.ProcessPasswordWithKeyVault(storedEncryptedPassword,
                        Constants.PASSWORD_ENCRYPTION_KEY, "decrypt");

                    if (decryptedPassword.Equals(data.password.ToString()))
                    {
                        log.LogInformation($"User logged in successfully: {data.userName}");
                        result = new OkObjectResult(requestBody);
                    }
                    else
                    {
                        log.LogInformation($"Invalid password for username {data.userName}");
                        result = new UnauthorizedObjectResult("Invalid username or password");
                    }
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