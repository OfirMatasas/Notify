using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GooglePlacesApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Core;
using Xamarin.Forms.Internals;
using Constants = Notify.Helpers.Constants;
using Notify.Helpers;

namespace Notify.HttpClient
{
    public class GoogleHttpClient
    { 
        private static GoogleHttpClient m_Instance;
        private static readonly object r_LockInstanceCreation = new object();
        private static System.Net.Http.HttpClient m_HttpClient;
        private static readonly string r_GoogleAPIkey = "AIzaSyCXUyen9sW3LhiELjOPJtUc0OqZlhLr-cg";

        private GoogleHttpClient()
        {
            m_HttpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(Constants.GOOGLE_BASE_URL),
                DefaultRequestHeaders =
                {
                    Accept = { new MediaTypeWithQualityHeaderValue("application/json")},
                    Authorization = new AuthenticationHeaderValue("Bearer", r_GoogleAPIkey)
                },
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            m_HttpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            m_HttpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
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
        
        private static readonly LoggerService r_Logger = LoggerService.Instance;
        private readonly System.Net.Http.HttpClient r_HttpClient;
        private static readonly string r_GoogleAPIkey = "AIzaSyCXUyen9sW3LhiELjOPJtUc0OqZlhLr-cg";
        }

        public GoogleHttpClient Uri(string uri)
        {
            m_HttpClient.BaseAddress = new Uri(uri);
            return this;
        }

        public async Task<string> GetAsync(string uri)
        {
            string content = null;
            HttpResponseMessage response;

            try
            {
                response = await m_HttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on GetAsync: {Environment.NewLine}{ex.Message}");
            }
            
            return content;
        }

        public async Task<List<String>> GetAddressSuggestions(string addressProvided)
        {
            string requestUri = $"place/autocomplete/json?input={addressProvided}&types=address&key={r_GoogleAPIkey}";
            List<string> suggestions = new List<string>();
            string response;
            JObject responseJson;
            JToken predictions;

            try
            {
                response = await GetAsync(requestUri);
                responseJson = JObject.Parse(response);
                predictions = responseJson["predictions"];
                predictions.ForEach(prediction => suggestions.Add(prediction["description"].ToString()));
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on GetAddressSuggestions: {ex.Message}");
            }

            return suggestions;
        }
        
        public async Task<Coordinates> GetCoordinatesFromAddress(string addressProvided)
        {
            string requestUri = $"geocode/json?address={addressProvided}&key={r_GoogleAPIkey}";
            Coordinates coordinates = null;
            string response;
            GeocodingResponse geocodingResponse;

            try
            {
                response = await GetAsync(requestUri);
                geocodingResponse = JsonConvert.DeserializeObject<GeocodingResponse>(response);
                
                if (geocodingResponse.Results.Count > 0)
                {
                    r_Logger.LogDebug($"eocodingResponse.Results.Count: {geocodingResponse.Results.Count}");
                    coordinates = geocodingResponse.Results[0].Geometry.Location;
                    Debug.WriteLine($"Coordinates from address provided: latitude: {coordinates.Lat}, longitude: {coordinates.Lng}");
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on GetLatLngFromAddress: {ex.Message}");
            }

            return coordinates;
        }
        
        public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            string requestUri = $"geocode/json?key={r_GoogleAPIkey}&latlng={latitude},{longitude}";
            string address = null;
            string response;
            string responseJson;
            GoogleMapsApiResult result;
    
            try
            {
                response = await GetAsync(requestUri);
                result = JsonConvert.DeserializeObject<GoogleMapsApiResult>(response);

                if (result == null || result.GoogleMapsResults.Length == 0)
                {
                    address = "Unknown address";
                    r_Logger.LogDebug($"Unknown address");
                }
                else
                {
                    address = result.GoogleMapsResults[0].FormattedAddress;
                    r_Logger.LogDebug($"Current address: {address}");
                }
                
                Debug.WriteLine($"Address from coordinates provided: {address}");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on GetAddressFromCoordinatesAsync: {Environment.NewLine}{ex.Message}");
            }

            return address;
        }
        
        public async Task<List<Place>> SearchPlacesNearby(double latitude, double longitude, int radius, string type)
        {
            string requestUri = $"place/nearbysearch/json?key={r_GoogleAPIkey}&location={latitude},{longitude}&radius={radius}&type={type.ToLower()}";
            List<Place> places = new List<Place>();
            string response;
            JObject responseJson;
            JToken results;
            Place place;

            try
            {
                response = await GetAsync(requestUri);
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
                r_Logger.LogError($"Error occured on SearchPlacesNearby: {Environment.NewLine}{ex.Message}");
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
}
