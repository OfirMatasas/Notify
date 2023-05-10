using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;

namespace Notify.Functions.Utils
{
    public static class ConversionUtils
    {
        public static string ConvertBsonDocumentListToJson(List<BsonDocument> bsonDocumentList)
        {
            var jsonList = bsonDocumentList.Select(bsonDocument =>
            {
                var jsonSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                return bsonDocument.ToJson(jsonSettings);
            }).ToList();

            var jsonArrayString = $"[{string.Join(',', jsonList)}]";
            var jsonArray = JArray.Parse(jsonArrayString);
            return jsonArray.ToString();
        }
    }
}
