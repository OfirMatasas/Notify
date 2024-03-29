using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Notify.Functions.Core;
using Notify.Functions.HTTPClients;

namespace Notify.Functions.Utils
{
    public static class ValidationUtils
    {
        public static bool ValidateUsername(HttpRequest request, ILogger logger)
        {
            bool valid = false;

            if (string.IsNullOrEmpty(request.Query["username"]))
            {
                logger.LogError("The 'username' query parameter is required");
            }
            else
            {
                valid = true;
            }

            return valid;
        }
        
        public static async Task<bool> CheckIfUserExistsAsync(string username)
        {
            IMongoCollection<BsonDocument> collection;
            FilterDefinition<BsonDocument> filterUsername;
            long countUsername;

            collection = AzureDatabaseClient.Instance
                .GetCollection<BsonDocument>(
                    databaseName: Constants.DATABASE_NOTIFY_MTA,
                    collectionName: Constants.COLLECTION_USER);
            filterUsername = Builders<BsonDocument>.Filter.Regex("userName",
                new BsonRegularExpression($"^{Regex.Escape(Convert.ToString(username))}$", "i"));

            countUsername = await collection.CountDocumentsAsync(filterUsername);
            return countUsername > 0;
        }
    }
}
