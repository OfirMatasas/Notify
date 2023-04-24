using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Helpers;
using Notify.Core;

namespace Notify.HttpClient
{
    public class AzureHttpClient
    {
        private static AzureHttpClient m_Instance;
        private static readonly object r_LockInstanceCreation = new object();
        private static System.Net.Http.HttpClient m_HttpClient;

        private AzureHttpClient()
        {
            m_HttpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(Constants.AZURE_FUNCTIONS_APP_BASE_URL)
            };
        }

        public static AzureHttpClient Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (r_LockInstanceCreation)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new AzureHttpClient();
                        }
                    }
                }

                return m_Instance;
            }
        }

        public bool CheckIfArrivedDestination(Location location)
        {
            dynamic request = new JObject();
            string json;
            HttpResponseMessage response;
            dynamic returnedObject;
            double distance;
            bool arrived;

            try
            {
                request.location = new JObject();
                request.location.latitude = location.Latitude;
                request.location.longitude = location.Longitude;
                json = JsonConvert.SerializeObject(request);
                Debug.WriteLine($"request:{Environment.NewLine}{request}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_DISTANCE, 
                    content: createJsonStringContent(json)
                    ).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from GetDistanceToDestinationFromCurrentLocation, location: {location}!");

                returnedObject = DeserializeObjectFromResponseAsync(response).Result;
                distance = Convert.ToDouble(returnedObject.distance);
                Debug.WriteLine($"distance: {distance}");

                arrived = distance <= Constants.DESTINATION_MAXMIMUM_DISTANCE;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on GetDistanceToDestinationFromCurrentLocation, {location}:{Environment.NewLine}{ex.Message}");
                arrived = false;
            }

            return arrived;
        }

        private async Task<HttpResponseMessage> postAsync(string requestUri, StringContent content)
        {
            HttpResponseMessage response = await m_HttpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            return response;
        }

        private async Task<dynamic> DeserializeObjectFromResponseAsync(HttpResponseMessage response)
        {
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(responseJson);
        }

        private StringContent createJsonStringContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public bool CreateTimeNotification(string notificationName, string notificationType, DateTime dateTime, List<string> users)
        {
            dynamic request = new JObject();
            string json;
            HttpResponseMessage response;
            bool created;

            try
            {
                request.creator = "Ofir";
                request.notification = new JObject();
                request.notification.name = notificationName;
                request.notification.type = notificationType;
                request.notification.date = dateTime.Date;
                request.notification.time = dateTime.TimeOfDay;
                request["users"] = JToken.FromObject(users);

                json = JsonConvert.SerializeObject(request);
                Debug.WriteLine($"request:{Environment.NewLine}{request}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from CreateTimeNotification!");
                created = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on CreateTimeNotification:{Environment.NewLine}{ex.Message}");
                created = false;
            }

            return created;
        }

        public bool CreateLocationNotification(string notificationName, string notificationType, string location, List<string> users)
        {
            dynamic request = new JObject();
            string json;
            HttpResponseMessage response;
            bool created;

            try
            {
                request.creator = "Ofir";
                request.notification = new JObject();
                request.notification.name = notificationName;
                request.notification.type = notificationType;
                request.notification.date = location;
                request["users"] = JToken.FromObject(users);

                json = JsonConvert.SerializeObject(request);
                Debug.WriteLine($"request:{Environment.NewLine}{request}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from CreateLocationNotification!");
                created = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on CreateLocationNotification:{Environment.NewLine}{ex.Message}");
                created = false;
            }

            return created;
        }
    }
}
