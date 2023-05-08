using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Notify.Functions.Utils;

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
            IndexKeysDefinition<BsonDocument> indexKeys = Builders<BsonDocument>.IndexKeys.Ascending(propertyToIndex);
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
            Debug.WriteLine(
                $"Index created for property '{propertyToIndex}' in collection '{collection.CollectionNamespace.CollectionName}'");
        }
    }
    
    public static string RenderToJson<TDocument>(this FilterDefinition<TDocument> filter)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<TDocument>();
        var renderedFilter = filter.Render(documentSerializer, serializerRegistry);

        if (renderedFilter.ElementCount == 1)
        {
            var field = renderedFilter.Elements.ElementAt(0).Name;
            var value = renderedFilter.Elements.ElementAt(0).Value;
            
            return $"{field} '{value}'";
        }

        return $"User already exists with multiple fields: {renderedFilter.ToJson()}";
    }

}