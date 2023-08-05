using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Notify.Functions.Core;
using Notify.Functions.HTTPClients;

namespace Notify.Functions.Utils
{
    public static class MongoUtils
    {
        public static async Task CreatePropertyIndexesAsync(IMongoCollection<BsonDocument> collection,
            params string[] propertiesToIndex)
        {
            foreach (string propertyToIndex in propertiesToIndex)
            {
                CreateIndexOptions indexOptions = new CreateIndexOptions
                {
                    Unique = true
                };
                IndexKeysDefinition<BsonDocument> indexKeys =
                    Builders<BsonDocument>.IndexKeys.Ascending(propertyToIndex);
                await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
                Debug.WriteLine(
                    $"Index created for property '{propertyToIndex}' in collection '{collection.CollectionNamespace.CollectionName}'");
            }
        }
        
        public static IMongoCollection<BsonDocument> GetCollection(string collectionName)
        {
            IMongoDatabase database;

            database = AzureDatabaseClient.Instance.GetDatabase(Constants.DATABASE_NOTIFY_MTA);
            return  database.GetCollection<BsonDocument>(collectionName);
        }
    }
}
