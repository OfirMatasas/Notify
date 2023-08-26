using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Notify.Functions.Utils
{
    public static class ConversionUtils
    {
        public static string ConvertBsonDocumentListToJson(List<BsonDocument> bsonDocumentList)
        {
            JsonWriterSettings jsonSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
            string jsonArrayString, id;
            JArray jsonArray;

            List<string> jsonList = bsonDocumentList.Select(document =>
            {
                if (document.Contains("_id"))
                {
                    id = document.GetValue("_id").AsObjectId.ToString();
                    document.Remove("_id");
                    document.Add("id", id);
                }

                return document.ToJson(jsonSettings);
            }).ToList();

            jsonArrayString = $"[{string.Join(',', jsonList)}]";
            jsonArray = JArray.Parse(jsonArrayString);
            
            return jsonArray.ToString();
        }

        public static async Task<dynamic> ExtractBodyContentAsync(HttpRequest request)
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject(requestBody);
        }
    }
}
