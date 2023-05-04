using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Core;
using Notify.Helpers;

namespace Notify.Azure.HttpClient
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

        public bool updateDestination(string destinationName, Location location)
        {
            dynamic data = new JObject();
            string json;
            HttpResponseMessage response;
            bool isSuccess;

            try
            {
                data.user = "Lin"; // TODO: get the user name from the logged in user
                data.location = new JObject();

                data.location.name = destinationName;
                data.location.longitude = location.Longitude;
                data.location.latitude = location.Latitude;

                json = JsonConvert.SerializeObject(data);
                Debug.WriteLine($"request:{Environment.NewLine}{data}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_DESTINATION_UPDATE,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from updateDestination, for location: {location}!");
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on updateDestination: {Environment.NewLine}{ex.Message}");
                isSuccess = false;
            }

            return isSuccess;
        }
        
        public bool SendSMSVerificationCode(string telephoneNumber, string verificationCode)
        {
            dynamic data = new JObject();
            string json;
            HttpResponseMessage response;
            bool isSuccess;

            try
            {
                data.telephone = telephoneNumber;
                data.verificationCode = verificationCode;
                
                json = JsonConvert.SerializeObject(data);
                Debug.WriteLine($"request:{Environment.NewLine}{data}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_SEND_SMS,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from sendSMSVerificationCode, telephone: {telephoneNumber}, verificationCode: {verificationCode}");
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on sendSMSVerificationCode: {Environment.NewLine}{ex.Message}");
                isSuccess = false;
            }

            return isSuccess;
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

        public bool CreateTimeNotification(string notificationName, string description, string notificationType, 
            DateTime dateTime, List<string> users)
        {
            long timestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

            return createNotification(notificationName, description, notificationType, "timestamp", timestamp, users, 
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_TIME);
        }
        
        public bool CreateLocationNotification(string notificationName, string description, string notificationType, 
            string location, List<string> users)
        {
            return createNotification(notificationName, description, notificationType, "location", location, users, 
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_LOCATION);
        }

        private bool createNotification(string notificationName, string description, string notificationType, 
            string key, JToken value, List<string> users, string uri)
        {
            string json;
            HttpResponseMessage response;
            bool created;

            try
            {
                json = createJsonOfNotificationRequest(notificationName, description, notificationType, key, value , users);
                Debug.WriteLine($"request:{Environment.NewLine}{json}");

                response = postAsync(uri, createJsonStringContent(json)).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from createNotification!");
                created = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on createNotification: {ex.Message}");
                created = false;
            }

            return created;
        }

        private string createJsonOfNotificationRequest(string notificationName, string description, string notificationType,
            string key, JToken value, List<string> users)
        {
            dynamic request = new JObject
            {
                { "creator", "Ofir" /* TODO: Get username from current logged in user */ },
                { "description", description?.Trim() },
                {
                    "notification", new JObject
                    {
                        { "name", notificationName?.Trim() },
                        { "type", notificationType },
                        { key, value }
                    }
                },
                { "users", JToken.FromObject(users) }
            };

            return JsonConvert.SerializeObject(request);
        }

        public async Task<List<Notification>> GetNotifications()
        {
            List<Notification> notifications = new List<Notification>();
            NotificationType notificationType;
            object notificationTypeValue;

            try
            {
                HttpResponseMessage response = getAsync(Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION).Result;
                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from GetNotifications!");

                dynamic returnedObject = DeserializeObjectFromResponseAsync(response).Result;
                Debug.WriteLine($"returnedObject:\n{returnedObject.ToString()}");
                
                foreach (dynamic item in returnedObject)
                {
                    notificationType = item.notification.location == null ? NotificationType.Time : NotificationType.Location;
                    notificationTypeValue = item.notification.location ?? DateTimeOffset.FromUnixTimeSeconds((long)item.notification.timestamp).LocalDateTime;
                    
                    Notification notification = new Notification(
                        name: (string)item.notification.name,
                        description: (string)(item.description ?? item.info),
                        creationDateTime: DateTimeOffset.FromUnixTimeSeconds((long)item.creation_timestamp).LocalDateTime,
                        status: (string)item.status,
                        creator: (string)item.creator,
                        type: notificationType,
                        typeInfo: notificationTypeValue,
                        target: (string)item.user);

                    notifications.Add(notification);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on GetNotifications: {ex.Message}");
            }

            return notifications;
        }
        
        private async Task<HttpResponseMessage> getAsync(string requestUri)
        {
            HttpResponseMessage response = await m_HttpClient.GetAsync(requestUri).ConfigureAwait(false);
            return response;
        }

        public bool CheckIfCredentialsAreValid(string userName, string password)
        {
            bool validCredentials;
            dynamic request = new JObject();
            string json;
            HttpResponseMessage response;

            try
            {
                request.userName = userName;
                request.password = password;
                Debug.WriteLine($"request: {request}");

                json = JsonConvert.SerializeObject(request);
                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_LOGIN, createJsonStringContent(json)).Result;
                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from CheckIfCredentialsAreValid!");

                validCredentials = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on CheckIfCredentialsAreValid: {ex.Message}");
                validCredentials = false;
            }

            return validCredentials;
        }
    }
}
