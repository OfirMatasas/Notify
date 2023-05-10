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
using Xamarin.Essentials;
using Location = Notify.Core.Location;

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

        public async Task<bool> UpdateDestination<T>(string destinationName, T locationData)
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

                if (locationData is Location location)
                {
                    data.location.type = NotificationType.Location.ToString();
                    data.location.longitude = location.Longitude;
                    data.location.latitude = location.Latitude;
                }
                else if (locationData is string wifiSsid)
                {
                    data.location.type = NotificationType.WiFi.ToString();
                    data.location.ssid = wifiSsid;
                }
                else
                {
                    throw new ArgumentException($"Invalid locationData type: {typeof(T).FullName}");
                }

                json = JsonConvert.SerializeObject(data);
                Debug.WriteLine($"request:{Environment.NewLine}{data}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_DESTINATION_UPDATE,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                Debug.WriteLine($"Successful status code from Azure Function from UpdateDestination for {destinationName}");
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred on UpdateDestination: {Environment.NewLine}{ex.Message}");
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
                Debug.WriteLine($"Successful status code from Azure Function from createNotification");
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
                { "creator", Constants.USER_NAME /* TODO: Get username from current logged in user */ },
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

        private async Task<HttpResponseMessage> getAsync(string requestUri, string query = "")
        {
            Debug.WriteLine($"Sending HTTP GET request to {requestUri + query} endpoint");
            HttpResponseMessage response = await m_HttpClient.GetAsync(requestUri + query).ConfigureAwait(false);
            
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
                Debug.WriteLine($"Successful status code from Azure Function from CheckIfCredentialsAreValid");

                validCredentials = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on CheckIfCredentialsAreValid: {ex.Message}");
                validCredentials = false;
            }

            return validCredentials;
        }

        private async Task<List<T>> GetData<T>(string endpoint, string preferencesKey, Func<dynamic, T> converter)
        {
            string query = $"?username={Constants.USER_NAME}";
            HttpResponseMessage response;
            dynamic returnedObject;
            List<T> data = new List<T>();
                    
            Debug.WriteLine($"Getting data from endpoint {endpoint}");

            try
            {
                response = await getAsync(endpoint, query);
                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from {endpoint}!");

                returnedObject = await DeserializeObjectFromResponseAsync(response);
                Debug.WriteLine($"Returned object from {endpoint}:\n{returnedObject.ToString()}");

                foreach (dynamic item in returnedObject)
                {
                    T itemConverted = converter(item);
                    data.Add(itemConverted);
                }

                Preferences.Set(preferencesKey, JsonConvert.SerializeObject(data));
                Debug.WriteLine($"{preferencesKey} from {endpoint} was saved in preferences");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred on {endpoint}: {ex.Message}");
            }

            return data;
        }

        public async Task<List<Notification>> GetNotifications()
        {
            return await GetData(Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION, Constants.PREFRENCES_NOTIFICATIONS, item => {
                NotificationType notificationType = item.notification.location == null ? NotificationType.Time : NotificationType.Location;
                object notificationTypeValue = item.notification.location ?? DateTimeOffset.FromUnixTimeSeconds((long)item.notification.timestamp).LocalDateTime;

                return new Notification(
                    name: (string)item.notification.name,
                    description: (string)(item.description ?? item.info),
                    creationDateTime: DateTimeOffset.FromUnixTimeSeconds((long)item.creation_timestamp).LocalDateTime,
                    status: (string)item.status,
                    creator: (string)item.creator,
                    type: notificationType,
                    typeInfo: notificationTypeValue,
                    target: (string)item.user);
            });
        }

        public async Task<List<Friend>> GetFriends()
        {
            return await GetData(Constants.AZURE_FUNCTIONS_PATTERN_FRIEND, Constants.PREFRENCES_FRIENDS, item => new Friend(
                name: (string)item.name,
                userName: (string)item.userName,
                telephone: (string)item.telephone));
        }

        public async Task<List<Destination>> GetDestinations()
        {
            return await GetData(Constants.AZURE_FUNCTIONS_PATTERN_DESTINATIONS, Constants.PREFRENCES_DESTINATIONS,
                item => 
                {
                    Destination destination = new Destination(item.name)
                    {
                        Location = new Location((double)(item.latitude ?? 0), (double)(item.longitude ?? 0)),
                        SSID = item.ssid ?? "",
                        Bluetooth = item.bluetooth ?? ""
                    };
                    return destination;
                });
        }
    }
}
