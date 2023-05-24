using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Helpers;

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
        
        private readonly LoggerService r_logger = LoggerService.Instance;
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
            HttpResponseMessage response;

            try
            {
                response = await r_HttpClient.GetAsync(r_HttpClient.BaseAddress);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                r_logger.LogError($"Error occured on Execute: {Environment.NewLine}{ex.Message}");
            }
            
            return content;
        }

        public static async Task<List<String>> GetAddressSuggestions(string subAddress, LoggerService i_logger)
        {
            string requestUrl =
                $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={subAddress}&types=address&key={r_GoogleAPIkey}";
            List<string> suggestions = new List<string>();
            string response, address;
            JObject responseJson;
            JToken predictions;

            try
            {
                response = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method()
                    .Execute();
                responseJson = JObject.Parse(response);
                predictions = responseJson["predictions"];
                foreach (JToken prediction in predictions)
                {
                    address = prediction["description"].ToString();
                    suggestions.Add(address);
                    i_logger.LogDebug(address);
                }
            }
            catch (Exception ex)
            {
                i_logger.LogError($"Error occured on GetAddressSuggestions: {ex.Message}");
            }

            return suggestions;
        }
        
        public static async Task<Coordinates> GetCoordinatesFromAddress(string address, LoggerService i_logger)
        {
            string requestUrl =
                $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={r_GoogleAPIkey}";
            Coordinates coordinates = null;
            string response;
            GeocodingResponse geocodingResponse;

            try
            {
                response = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method()
                    .Execute();
                geocodingResponse = JsonConvert.DeserializeObject<GeocodingResponse>(response);
                if (geocodingResponse.Results.Count > 0)
                {
                    i_logger.LogDebug($"eocodingResponse.Results.Count: {geocodingResponse.Results.Count}");
                    coordinates = geocodingResponse.Results[0].Geometry.Location;
                }
            }
            catch (Exception ex)
            {
                i_logger.LogError($"Error occured on GetLatLngFromAddress: {ex.Message}");
            }

            return coordinates;
        }
        
        public static async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude, LoggerService i_logger)
        {
            string requestUrl = 
                $"https://maps.googleapis.com/maps/api/geocode/json?key={r_GoogleAPIkey}&latlng={latitude},{longitude}";
            string address = null;
            HttpResponseMessage response;
            string responseJson;
            GoogleMapsApiResult result;
    
            try
            {
                response = await Builder()
                    .Uri(requestUrl)
                    .Method()
                    .r_HttpClient
                    .GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                responseJson = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<GoogleMapsApiResult>(responseJson);

                if (result == null || result.GoogleMapsResults.Length == 0)
                {
                    address = "Unknown address";
                    i_logger.LogDebug($"Unknown address");
                }
                else
                {
                    address = result.GoogleMapsResults[0].FormattedAddress;
                    i_logger.LogDebug($"Current address: {address}");
                }
            }
            catch (Exception ex)
            {
                i_logger.LogError($"Error occured on GetAddressFromCoordinatesAsync: {Environment.NewLine}{ex.Message}");
            }

            return address;
        }
        
        public static async Task<List<Place>> SearchPlacesNearby(double latitude, double longitude, int radius, string type, LoggerService i_logger)
        {
            string requestUrl = 
                $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?key={r_GoogleAPIkey}&location={latitude},{longitude}&radius={radius}&type={type.ToLower()}";
            List<Place> places = new List<Place>();
            string response;
            JObject responseJson;
            JToken results;
            Place place;

            try
            {
                response = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method()
                    .Execute();
                responseJson = JObject.Parse(response);
                results = responseJson["results"];
                
                foreach (JToken result in results)
                {
                    place = new Place();
                    place.Name = result["name"].ToString();
                    place.PlaceId = result["place_id"].ToString();
                    place.Latitude = result["geometry"]["location"]["lat"].ToObject<double>();
                    place.Longitude = result["geometry"]["location"]["lng"].ToObject<double>();
                    places.Add(place);
                }
            }
            catch (Exception ex)
            {
                i_logger.LogError($"Error occured on SearchPlacesNearby: {Environment.NewLine}{ex.Message}");
            }

            return places;
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
        
        private class GoogleMapsApiResult
        {
            [JsonProperty("results")]
            public GoogleMapsApiResultItem[] GoogleMapsResults { get; set; }
        }

        private class GoogleMapsApiResultItem
        {
            [JsonProperty("formatted_address")]
            public string FormattedAddress { get; set; }
        }
    }
    
    public class Place
    {
        public string Name { get; set; }
        public string PlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
