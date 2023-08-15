using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureVault;
using Constants = Notify.Functions.Core.Constants;

namespace Notify.Functions.HTTPClients
{
    public class GoogleHttpClient
    { 
        private static GoogleHttpClient m_Instance;
        private static readonly object r_LockInstanceCreation = new object();
        private static HttpClient m_HttpClient;

        private GoogleHttpClient()
        {
            m_HttpClient = new HttpClient
            {
                BaseAddress = new Uri(Constants.GOOGLE_API_BASE_URL),
                DefaultRequestHeaders =
                {
                    Accept = { new MediaTypeWithQualityHeaderValue("application/json")},
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
        }

        private async Task<string> GetAsync(string uri, ILogger logger)
        {
            string googleAPIkey = await AzureVault.GetSecretFromVault(Constants.GOOGLE_API_KEY);
            string content = null;
            HttpResponseMessage response;

            try
            {
                m_HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", googleAPIkey);
                response = await m_HttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured on GetAsync: {Environment.NewLine}{ex.Message}");
            }
            
            return content;
        }

        public async Task<List<string>> GetAddressSuggestionsAsync(string addressProvided, ILogger logger)
        {
            string googleAPIkey = await AzureVault.GetSecretFromVault(Constants.GOOGLE_API_KEY);
            string requestUri = $"place/autocomplete/json?input={addressProvided}&types=address&key={googleAPIkey}";
            List<string> suggestions = new List<string>();
            string response;
            JObject responseJson;
            JToken predictions;

            try
            {
                response = await GetAsync(requestUri, logger);
                responseJson = JObject.Parse(response);
                predictions = responseJson["predictions"];

                foreach (JToken prediction in predictions)
                {
                    suggestions.Add(prediction["description"].ToString());
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured on GetAddressSuggestions: {Environment.NewLine}{ex.Message}");
            }

            return suggestions;
        }
        
        public async Task<Coordinates> GetCoordinatesFromAddressAsync(string addressProvided, ILogger logger)
        {
            string googleAPIkey = await AzureVault.GetSecretFromVault(Constants.GOOGLE_API_KEY);
            string requestUri = $"geocode/json?address={addressProvided}&key={googleAPIkey}";
            Coordinates coordinates = null;
            string response;
            GeocodingResponse geocodingResponse;

            try
            {
                response = await GetAsync(requestUri, logger);
                geocodingResponse = JsonConvert.DeserializeObject<GeocodingResponse>(response);
                
                if (geocodingResponse.Results.Count > 0)
                {
                    coordinates = geocodingResponse.Results[0].Geometry.Location;
                    logger.LogInformation($"Coordinates from address provided: latitude: {coordinates.Lat}, longitude: {coordinates.Lng}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured on GetCoordinatesFromAddress: {Environment.NewLine}{ex.Message}");
            }

            return coordinates;
        }
        
        public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude, ILogger logger)
        {
            string googleAPIkey = await AzureVault.GetSecretFromVault(Constants.GOOGLE_API_KEY);
            string requestUri = $"geocode/json?key={googleAPIkey}&latlng={latitude},{longitude}";
            string address = null;
            string response;
            GoogleMapsApiResult result;
    
            try
            {
                response = await GetAsync(requestUri, logger);
                result = JsonConvert.DeserializeObject<GoogleMapsApiResult>(response);

                if (result == null || result.GoogleMapsResults.Length == 0)
                {
                    address = "Unknown address";
                }
                else
                {
                    address = result.GoogleMapsResults[0].FormattedAddress;
                }
                
                logger.LogInformation($"Address from coordinates provided: {address}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured on GetAddressFromCoordinatesAsync: {Environment.NewLine}{ex.Message}");
            }

            return address;
        }
        
        public async Task<List<Place>> SearchPlacesNearby(double latitude, double longitude, int radius, string type, ILogger logger)
        {
            string googleAPIkey = await AzureVault.GetSecretFromVault(Constants.GOOGLE_API_KEY);
            string requestUri = $"place/nearbysearch/json?key={googleAPIkey}&location={latitude},{longitude}&radius={radius}&type={type.ToLower()}";
            List<Place> places = new List<Place>();
            string response;
            JObject responseJson;
            JToken results;
            Place place;

            try
            {
                response = await GetAsync(requestUri, logger);
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
                logger.LogError($"Error occured on SearchPlacesNearby: {Environment.NewLine}{ex.Message}");
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
