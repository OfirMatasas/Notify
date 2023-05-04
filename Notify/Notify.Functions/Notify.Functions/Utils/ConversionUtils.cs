using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace Notify.Functions.Utils
{
    public class ConversionUtils
    {
        public static string ConvertBsonDocumentListToJson(List<BsonDocument> bsonDocumentList)
        {
            string json = bsonDocumentList.ToList()
                .ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            
            return json;
        }
    }
}
