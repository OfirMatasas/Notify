using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Notify.HttpClient
{
    public class GoogleHttpClient
    {
        // TODO:
        //      1. Delete the statics and create m_HttpClient ad in Azure
        //      2. Insert google API key to vault
        //      3. Make m_HttpClient singleton
        //      4. Move to functions

        /*private static GoogleHttpClient m_Instance;
        private static readonly object r_LockInstanceCreation = new object();
        private static System.Net.Http.HttpClient m_HttpClient;
       
       
        private GoogleHttpClient()
        {
            m_HttpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri("https://maps.googleapis.com");
            };
        }
       
        public static GoogleHttpClient Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (r_LockInstanceCreation)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new GoogleHttpClient();
                        }
                    }
                }
       
                return m_Instance;
            }
        }*/

        private readonly System.Net.Http.HttpClient r_HttpClient;
        private static readonly string r_GoogleAPIkey = "AIzaSyCXUyen9sW3LhiELjOPJtUc0OqZlhLr-cg";

        private GoogleHttpClient()
        {
            r_HttpClient = new System.Net.Http.HttpClient();
        }

        public static GoogleHttpClient Builder()
        {
            return new GoogleHttpClient();
        }

        public GoogleHttpClient Uri(string uri)
        {
            Debug.WriteLine($"In Uri function: {uri}");
            r_HttpClient.BaseAddress = new Uri(uri);
            return this;
        }

        public GoogleHttpClient Method()
        {
            r_HttpClient.DefaultRequestHeaders.Accept.Clear();
            r_HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            r_HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", r_GoogleAPIkey);
            r_HttpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            r_HttpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            r_HttpClient.Timeout = TimeSpan.FromSeconds(30);
            return this;
        }

        public async Task<string> Execute()
        {
            string content = null;

            try
            {
                HttpResponseMessage response = await r_HttpClient.GetAsync(r_HttpClient.BaseAddress);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on Execute: {Environment.NewLine}{ex.Message}");
            }
            
            return content;
        }

        public static async Task<List<String>> GetAddressSuggestions(string subAddress)
        {
            string requestUrl =
                $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={subAddress}&types=address&key={r_GoogleAPIkey}";
            List<string> suggestions = new List<string>();

            try
            {
                string stringContent = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method()
                    .Execute();
                JObject responseJson = JObject.Parse(stringContent);
                JToken predictions = responseJson["predictions"];
                foreach (JToken prediction in predictions)
                {
                    string address = prediction["description"].ToString();
                    suggestions.Add(address);
                    Debug.WriteLine(address);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on GetAddressSuggestions: {Environment.NewLine}{ex.Message}");
            }

            return suggestions;
        }


        public static async Task<Coordinates> GetCoordinatesFromAddress(string address)
        {
            string requestUrl =
                $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={r_GoogleAPIkey}";
            Coordinates coordinates = null;

            try
            {
                string stringContent = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method()
                    .Execute();
                GeocodingResponse geocodingResponse = JsonConvert.DeserializeObject<GeocodingResponse>(stringContent);
                if (geocodingResponse.Results.Count > 0)
                {
                    Debug.WriteLine($"eocodingResponse.Results.Count: {geocodingResponse.Results.Count}");
                    coordinates = geocodingResponse.Results[0].Geometry.Location;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on GetLatLngFromAddress: {Environment.NewLine}{ex.Message}");
            }

            return coordinates;
        }
        
        public class GeocodingResponse
        {
            [JsonProperty("results")] public List<GeocodingResult> Results { get; set; }
        }

        public class GeocodingResult
        {
            [JsonProperty("geometry")] public GeocodingGeometry Geometry { get; set; }
        }

        public class GeocodingGeometry
        {
            [JsonProperty("location")] public Coordinates Location { get; set; }
        }

        public class Coordinates
        {
            [JsonProperty("lat")] public double Lat { get; set; }
            [JsonProperty("lng")] public double Lng { get; set; }
        }
    }
}
