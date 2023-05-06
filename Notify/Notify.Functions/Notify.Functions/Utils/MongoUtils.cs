using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Notify.Functions.Utils;

public static class MongoUtils
{
    public static async Task CreatePropertyIndex(IMongoCollection<BsonDocument> collection, string propertyToIndex)
    {
        CreateIndexOptions indexOptions = new CreateIndexOptions
        {
            Unique = true
        };
        IndexKeysDefinition<BsonDocument> indexKeys = Builders<BsonDocument>.IndexKeys.Ascending(propertyToIndex);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
    }
}