using System;
using System.IO;
using System.Text.RegularExpressions;
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
using Notify.Functions.HTTPClients;
using Notify.Functions.Utils;

namespace Notify.Functions.NotifyFunctions.Login
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
            FilterDefinition<BsonDocument> filterUsername, filterTelephone;
            long countUsername, countTelephone;

            log.LogInformation("Got client's HTTP request to check if user exists");

            try
            {
                collection = Utils.MongoUtils.GetCollection(Constants.COLLECTION_USER);
                log.LogInformation(
                    $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");
                
                data = await ConversionUtils.ExtractBodyContent(req);
                log.LogInformation($"Data:{Environment.NewLine}{data}");

                if (data.userName == null || data.telephone == null)
                {
                    result = new BadRequestObjectResult("Username and telephone are required.");
                }
                else
                {
                    filterUsername = Builders<BsonDocument>.Filter.Regex("userName",
                        new BsonRegularExpression($"^{Regex.Escape(Convert.ToString(data.userName))}$", "i"));
                    filterTelephone = Builders<BsonDocument>.Filter.Eq("telephone", Convert.ToString(data.telephone));

                    countUsername = await collection.CountDocumentsAsync(filterUsername);
                    countTelephone = await collection.CountDocumentsAsync(filterTelephone);

                    if (countUsername > 0 && countTelephone > 0)
                    {
                        result = new ConflictObjectResult(
                            $"User with username '{data.userName}' and telephone '{data.telephone}' already exists.");
                    }
                    else if (countUsername > 0)
                    {
                        result = new ConflictObjectResult($"User with username '{data.userName}' already exists.");
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
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                result = new BadRequestObjectResult($"Failed to check if user exists: {ex.Message}");
            }

            return result;
        }
    }
}
