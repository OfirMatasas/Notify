using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

        public bool RegisterUser(string name, string userName, string password, string telephone)
        {
            dynamic data = new JObject();
            string json;
            HttpResponseMessage response;
            bool registered;

            try
            {
                data.name = name;
                data.userName = userName;
                data.password = password;
                data.telephone = telephone;

                json = JsonConvert.SerializeObject(data);
                Debug.WriteLine($"request:{Environment.NewLine}{data}");

                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_REGISTER, createJsonStringContent(json)).Result;
                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from RegisterUser, name: {name}, userName: {userName}, password: {password}, telephone: {telephone}");

                registered = true;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error occurred on RegisterUser: {ex.Message}");
                registered = false;
            }

            return registered;
        }

        public bool CheckUserExists(string userName, string telephone, out string errorMessage)
        {
            dynamic data = new JObject();
            string json;
            HttpResponseMessage response;
            bool userExists = false;
            errorMessage = string.Empty;

            try
            {
                data.username = userName;
                data.telephone = telephone;

                json = JsonConvert.SerializeObject(data);
                Debug.WriteLine($"request:{Environment.NewLine}{data}");

                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_CHECK_USER_EXISTS, createJsonStringContent(json)).Result;
        
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    errorMessage = response.Content.ReadAsStringAsync().Result;
                    userExists = true;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                    Debug.WriteLine($"Successful status code from Azure Function from CheckUserExists");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error occurred on CheckUserExists: {ex.Message}");
                errorMessage = ex.Message;
                userExists = true;
            }

            return userExists;
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

        public bool CreateDynamicNotification(string notificationName, string description, string notificationType, 
            string dynamicLocation, List<string> users)
        {
            return createNotification(notificationName, description, notificationType, "location", dynamicLocation, users, 
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_DYNAMIC);
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
            string userName = Preferences.Get(Constants.PREFERENCES_USERNAME, "");
            dynamic request = new JObject
            {
                { "creator", userName },
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
                
                userName = response.Content.ToString();
                Preferences.Set(Constants.PREFERENCES_USERNAME, userName);
                
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
            string userName = Preferences.Get(Constants.PREFERENCES_USERNAME, "");
            string query = $"?username={userName}";
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
            return await GetData(
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION,
                preferencesKey: Constants.PREFERENCES_NOTIFICATIONS, 
                converter:  notification => { 
                    NotificationType notificationType = notification.notification.location == null ? NotificationType.Time : NotificationType.Location; 
                    object notificationTypeValue = notification.notification.location ?? DateTimeOffset.FromUnixTimeSeconds((long)notification.notification.timestamp).LocalDateTime;
                    
                    return new Notification(
                        id: (string)notification.id,
                        name: (string)notification.notification.name, 
                        description: (string)(notification.description ?? notification.info), 
                        creationDateTime: DateTimeOffset.FromUnixTimeSeconds((long)notification.creation_timestamp).LocalDateTime, 
                        status: (string)notification.status, 
                        creator: (string)notification.creator, 
                        type: notificationType, 
                        typeInfo: notificationTypeValue, 
                        target: (string)notification.user);
            });
        }

        public async Task<List<Friend>> GetFriends()
        {
            return await GetData(
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_FRIEND, 
                preferencesKey: Constants.PREFERENCES_FRIENDS, 
                converter: friend => new Friend(
                    name: (string)friend.name, 
                    userName: (string)friend.userName, 
                    telephone: (string)friend.telephone));
        }

        public async Task<List<Destination>> GetDestinations()
        {
            return await GetData( 
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_DESTINATIONS, 
                preferencesKey: Constants.PREFERENCES_DESTINATIONS,
                converter: destination => new Destination((string)destination.location.name) 
                {
                    Location = new Location(
                        longitude: (double)(destination.location.longitude ?? 0), 
                        latitude: (double)(destination.location.latitude ?? 0)),
                    SSID = (string)(destination.location.ssid ?? ""),
                    Bluetooth = (string)(destination.location.bluetooth ?? "")
                });
        }

        public void UpdateNotificationsStatus(List<Notification> notifications, string sent)
        {
            List<string> notificationsIds = notifications.Select(notification => notification.ID).ToList();
            dynamic request;
            string json;
            HttpResponseMessage response;
            
            try
            {
                request = new JObject
                {
                    { "notifications", JToken.FromObject(notificationsIds) },
                    { "status", sent }
                };
                
                json = JsonConvert.SerializeObject(request);
                Debug.WriteLine($"request:{Environment.NewLine}{json}");

                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_STATUS, createJsonStringContent(json)).Result;

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"Successful status code from Azure Function from UpdateNotificationsStatus");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on UpdateNotificationsStatus: {ex.Message}");
            }
        }
    }
}
