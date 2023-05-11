using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Notify.Functions.Utils
{
    public static class MongoUtils
    {
        public static async Task CreatePropertyIndexes(IMongoCollection<BsonDocument> collection,
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
    }
}
