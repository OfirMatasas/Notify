﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
        //      1. Set Constants
        //      2. Delete the statics and create m_HttpClient ad in Azure

        /* private static GoogleHttpClient m_Instance;
        private static readonly object r_LockInstanceCreation = new object();
        private static System.Net.Http.HttpClient m_HttpClient;
        
        
        private GoogleHttpClient()
        {
             m_HttpClient = new System.Net.Http.HttpClient();
             m_HttpClient.BaseAddress = new Uri("https://maps.googleapis.com");
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

        private readonly System.Net.Http.HttpClient m_HttpClient;
        private static readonly string r_GoogleAPIkey = "AIzaSyCXUyen9sW3LhiELjOPJtUc0OqZlhLr-cg";

        private GoogleHttpClient()
        {
            m_HttpClient = new System.Net.Http.HttpClient();
        }

        public static GoogleHttpClient Builder()
        {
            return new GoogleHttpClient();
        }

        public GoogleHttpClient Uri(string uri)
        {
            m_HttpClient.BaseAddress = new Uri(uri);
            return this;
        }

        public GoogleHttpClient Method(HttpMethod method)
        {
            m_HttpClient.DefaultRequestHeaders.Accept.Clear();
            m_HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            m_HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", r_GoogleAPIkey);
            m_HttpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            m_HttpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            m_HttpClient.Timeout = TimeSpan.FromSeconds(30);
            return this;
        }

        public async Task<string> Execute()
        {
            var response = await m_HttpClient.GetAsync(m_HttpClient.BaseAddress);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public static async Task<List<String>> GetAddressSuggestions(string subAddress) // TODO - delete 'static'?
        {
            string requestUrl =
                $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={subAddress}&types=address&key={r_GoogleAPIkey}";
            List<string> suggestions = new List<string>();

            try
            {
                string stringContent = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method(HttpMethod.Get)
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


        public static async Task<LatLng> GetLatLngFromAddress(string address)
        {
            string requestUrl =
                $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={r_GoogleAPIkey}";
                // $"https://maps.googleapis.com/maps/api/geocode/json?address={WebUtility.UrlEncode(address)}&key={r_GoogleAPIkey}";
            LatLng latLng = null;

            try
            {
                string stringContent = await GoogleHttpClient.Builder()
                    .Uri(requestUrl)
                    .Method(HttpMethod.Get)
                    .Execute();
                GeocodingResponse geocodingResponse = JsonConvert.DeserializeObject<GeocodingResponse>(stringContent);
                if (geocodingResponse.Results.Count > 0)
                {
                    latLng = geocodingResponse.Results[0].Geometry.Location;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on GetLatLngFromAddress: {Environment.NewLine}{ex.Message}");
            }

            return latLng;
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
            [JsonProperty("location")] public LatLng Location { get; set; }
        }

        public class LatLng
        {
            [JsonProperty("lat")] public double Lat { get; set; }
            [JsonProperty("lng")] public double Lng { get; set; }
        }
    }
}