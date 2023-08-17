using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Core;
using Notify.Helpers;
using Notify.Services;
using Notify.WiFi;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = Notify.Core.Location;

namespace Notify.Azure.HttpClient
{
    public class AzureHttpClient
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
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

        public async Task<bool> UpdateDestination<T>(string destinationName, T locationData, NotificationType notificationType)
        {
            dynamic data = new JObject();
            string json, content;
            HttpResponseMessage response;
            bool isSuccess;

            try
            {
                data.user = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
                data.location = new JObject();
                data.location.name = destinationName;

                if (notificationType.Equals(NotificationType.Location))
                {
                    Location location = locationData as Location;
                    data.location.type = NotificationType.Location.ToString();
                    data.location.longitude = location?.Longitude;
                    data.location.latitude = location?.Latitude;
                }
                else if (notificationType.Equals(NotificationType.WiFi))
                {
                    string wifiSsid = locationData as string;
                    data.location.type = NotificationType.WiFi.ToString();
                    data.location.ssid = wifiSsid;
                }
                else if (notificationType.Equals(NotificationType.Bluetooth))
                {
                    string bluetoothName = locationData as string;
                    data.location.type = NotificationType.Bluetooth.ToString();
                    data.location.device = bluetoothName;
                }
                else
                {
                    throw new ArgumentException($"Invalid locationData type: {typeof(T).FullName}");
                }

                json = JsonConvert.SerializeObject(data);
                r_Logger.LogInformation($"request:{Environment.NewLine}{data}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_DESTINATION_UPDATE,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
                r_Logger.LogDebug(content);
                r_Logger.LogInformation($"Successful status code from Azure Function from UpdateDestination for {destinationName}");
                isSuccess = true;
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred on UpdateDestination: {Environment.NewLine}{ex.Message}");
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
                r_Logger.LogInformation($"request:{Environment.NewLine}{data}");

                response = postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_SEND_SMS,
                    content: createJsonStringContent(json)
                ).Result;

                response.EnsureSuccessStatusCode();
                r_Logger.LogInformation($"Successful status code from Azure Function from sendSMSVerificationCode, telephone: {telephoneNumber}, verificationCode: {verificationCode}");
                isSuccess = true;
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on sendSMSVerificationCode: {Environment.NewLine}{ex.Message}");
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
                r_Logger.LogInformation($"request:{Environment.NewLine}{data}");

                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_REGISTER, createJsonStringContent(json)).Result;
                response.EnsureSuccessStatusCode();
                r_Logger.LogInformation($"Successful status code from Azure Function from RegisterUser, name: {name}, userName: {userName}, password: {password}, telephone: {telephone}");

                registered = true;
            }
            catch (HttpRequestException ex)
            {
                r_Logger.LogError($"Error occurred on RegisterUser: {ex.Message}");
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
                data.userName = userName;
                data.telephone = telephone;

                json = JsonConvert.SerializeObject(data);
                r_Logger.LogInformation($"request:{Environment.NewLine}{data}");

                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_CHECK_USER_EXISTS, createJsonStringContent(json)).Result;
        
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    errorMessage = response.Content.ReadAsStringAsync().Result;
                    userExists = true;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                    r_Logger.LogDebug($"Successful status code from Azure Function from CheckUserExists");
                }
            }
            catch (HttpRequestException ex)
            {
                r_Logger.LogError($"Error occurred on CheckUserExists: {ex.Message}");
                errorMessage = ex.Message;
                userExists = true;
            }

            return userExists;
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
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION_TIME);
        }
        
        public bool CreateLocationNotification(string notificationName, string description, string notificationType, 
            string location, string activation, List<string> users, bool isPermanent)
        {
            return createNotification(notificationName, description, notificationType, "location", location, users, 
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION_LOCATION, activation, isPermanent);
        }

        public bool CreateDynamicNotification(string notificationName, string description, string notificationType, 
            string dynamicLocation, List<string> users)
        {
            return createNotification(notificationName, description, notificationType, "location", dynamicLocation, users, 
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_CREATION_DYNAMIC);
        }

        private bool createNotification(string notificationName, string description, string notificationType, 
            string key, JToken value, List<string> users, string uri, string activation = null, bool isPermanent = false)
        {
            string json;
            HttpResponseMessage response;
            bool created;

            try
            {
                json = createJsonOfNotificationCreationRequest(notificationName, description, notificationType, key, value , users, activation, isPermanent);
                r_Logger.LogInformation($"request:{Environment.NewLine}{json}");

                response = postAsync(uri, createJsonStringContent(json)).Result;

                response.EnsureSuccessStatusCode();
                r_Logger.LogDebug($"Successful status code from Azure Function from createNotification");
                created = true;
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on createNotification: {ex.Message}");
                created = false;
            }

            return created;
        }

        private string createJsonOfNotificationCreationRequest(string notificationName, string description, string notificationType,
            string key, JToken value, List<string> users, string activation, bool isPermanent)
        {
            string userName = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
            dynamic request = new JObject
            {
                { "creator", userName },
                { "description", description?.Trim() },
                {
                    "notification", new JObject
                    {
                        { "name", notificationName?.Trim() },
                        { "type", notificationType },
                        { key, value },
                        { "permanent", isPermanent }
                    }
                },
                { "users", JToken.FromObject(users) }
            };

            if (!activation.IsNullOrEmpty())
            {
                request.notification.activation = activation;
            }

            return JsonConvert.SerializeObject(request);
        }

        private async Task<HttpResponseMessage> getAsync(string requestUri, string query = null)
        {
            r_Logger.LogDebug($"Sending HTTP GET request to {requestUri + query} endpoint");
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
                r_Logger.LogDebug($"request: {request}");

                json = JsonConvert.SerializeObject(request);
                response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_LOGIN, createJsonStringContent(json)).Result;
                response.EnsureSuccessStatusCode();
                r_Logger.LogDebug($"Successful status code from Azure Function from CheckIfCredentialsAreValid");

                userName = response.Content.ReadAsStringAsync().Result;
                Preferences.Set(Constants.PREFERENCES_USERNAME, userName);
                
                validCredentials = true;
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on CheckIfCredentialsAreValid: {ex.Message}");
                validCredentials = false;
            }

            return validCredentials;
        }

        private async Task<List<T>> GetData<T>(string endpoint, string preferencesKey, Func<dynamic, T> converter)
        {
            string userName = Preferences.Get(Constants.PREFERENCES_USERNAME, String.Empty);
            string query = $"?username={userName}";
            HttpResponseMessage response;
            dynamic returnedObject;
            List<T> data = new List<T>();
                    
            r_Logger.LogDebug($"Getting data from endpoint {endpoint}");

            try
            {
                response = await getAsync(endpoint, query);
                response.EnsureSuccessStatusCode();
                r_Logger.LogDebug($"Successful status code from Azure Function from {endpoint}!");

                returnedObject = await DeserializeObjectFromResponseAsync(response);
                r_Logger.LogDebug($"Returned object from {endpoint}:\n{returnedObject.ToString()}");

                foreach (dynamic item in returnedObject)
                {
                    T itemConverted = converter(item);
                    data.Add(itemConverted);
                }

                Preferences.Set(preferencesKey, JsonConvert.SerializeObject(data));
                r_Logger.LogInformation($"{preferencesKey} from {endpoint} was saved in preferences");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred on {endpoint}: {ex.Message}");
            }

            return data;
        }

        public async Task<List<Notification>> GetNotifications()
        {
            List<Notification> notifications = await GetData(
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION,
                preferencesKey: Constants.PREFERENCES_NOTIFICATIONS, 
                converter: Converter.ToNotification);

            createNewDynamicDestinations(notifications);
            DependencyService.Get<IWiFiManager>().SendNotifications(null, null);

            return notifications;
        }

        private void createNewDynamicDestinations(List<Notification> notifications)
        {
            List<string> newDynamicDestinations;
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, string.Empty);
            List<Destination> destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
            
            newDynamicDestinations = Constants.DYNAMIC_PLACE_LIST.FindAll(place =>
            {
                bool isPlaceInDestinations = destinations.Any(destination => destination.Name.Equals(place));
                bool isPlaceInNotifications = notifications.Any(notification =>
                    notification.Type.Equals(NotificationType.Dynamic) && notification.TypeInfo.Equals(place));
                
                return !isPlaceInDestinations && isPlaceInNotifications;
            });
            
            if(newDynamicDestinations.Count > 0)
            {
                LoggerService.Instance.LogInformation($"New dynamic destinations: {string.Join(", ", newDynamicDestinations)}");
            }
            
            foreach (string dynamicDestination in newDynamicDestinations)
            {
                destinations.Add(new Destination(dynamicDestination, true));
            }
            
            LoggerService.Instance.LogDebug($"New destinations: {string.Join(", ", destinations.Select(destination => destination.Name))}");
            Preferences.Set(Constants.PREFERENCES_DESTINATIONS, JsonConvert.SerializeObject(destinations));
        }

        public async Task<List<User>> GetFriends()
        {
            return await GetData(
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_FRIEND, 
                preferencesKey: Constants.PREFERENCES_FRIENDS, 
                converter: Converter.ToFriend);
        }

        public async Task<List<Destination>> GetDestinations()
        {
            List<Destination> destinations = await GetData(
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_DESTINATION,
                preferencesKey: Constants.PREFERENCES_DESTINATIONS,
                converter: Converter.ToDestination);

            foreach (string dynamicDestination in Constants.DYNAMIC_PLACE_LIST)
            {
                destinations.Add(new Destination(dynamicDestination, true));
            }
            
            LoggerService.Instance.LogInformation($"Destinations: {string.Join(", ", destinations.Select(destination => destination.Name))}");
            
            Preferences.Set(Constants.PREFERENCES_DESTINATIONS, JsonConvert.SerializeObject(destinations));
            return destinations;
        }

        public bool UpdateNotificationsStatus(List<Notification> notifications, string newStatus)
        {
            List<string> notificationsIds = notifications.Select(notification => notification.ID).ToList();
            dynamic request;
            string json;
            HttpResponseMessage response;
            bool isSuccess;
            
            if(notificationsIds.Count == 0)
            {
                r_Logger.LogDebug($"No notifications to update");
                isSuccess = false;
            }
            else
            {
                try
                {
                    request = new JObject
                    {
                        { "notifications", JToken.FromObject(notificationsIds) },
                        { "status", newStatus }
                    };

                    json = JsonConvert.SerializeObject(request);
                    r_Logger.LogInformation($"request:{Environment.NewLine}{json}");

                    response = postAsync(Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_STATUS,
                        createJsonStringContent(json)).Result;

                    response.EnsureSuccessStatusCode();
                    r_Logger.LogDebug($"Successful status code from Azure Function from UpdateNotificationsStatus");
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    r_Logger.LogError($"Error occured on UpdateNotificationsStatus: {ex.Message}");
                    isSuccess = false;
                }
            }
            
            return isSuccess;
        }

        public async Task<Location> GetCoordinatesFromAddress(string selectedAddress)
        {
            string query = $"?address={selectedAddress}";
            string requestUri = Constants.AZURE_FUNCTIONS_PATTERN_DESTINATION_COORDINATES + query;
            HttpResponseMessage response;
            dynamic returnedObject;
            Location location = null;

            try
            {
                response = await m_HttpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                returnedObject = await DeserializeObjectFromResponseAsync(response);
                location = new Location(
                    longitude: Convert.ToDouble(returnedObject.longitude),
                    latitude: Convert.ToDouble(returnedObject.latitude));
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on GetAddressSuggestions: {Environment.NewLine}{ex.Message}");
            }
            
            return location;
        }

        public async Task<List<string>> GetAddressSuggestions(string searchAddress)
        {
            string query = $"?address={searchAddress}";
            string requestUri = Constants.AZURE_FUNCTIONS_PATTERN_DESTINATION_SUGGESTIONS + query;
            HttpResponseMessage response;
            string responseJson;
            List<string> suggestions;

            try
            {
                r_Logger.LogInformation($"Getting address suggestions from {requestUri}");
                response = await m_HttpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                
                responseJson = await response.Content.ReadAsStringAsync();
                suggestions = JsonConvert.DeserializeObject<List<string>>(responseJson);
                r_Logger.LogInformation($"Got {suggestions.Count} suggestions from {requestUri}:{Environment.NewLine}{string.Join($"{Environment.NewLine}- ", suggestions)}");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on GetAddressSuggestions: {Environment.NewLine}{ex.Message}");
                suggestions = new List<string>();
            }
            
            return suggestions;
        }

        public async Task<List<User>> GetNotFriendsUsers()
        {
            return await GetData(
                endpoint: Constants.AZURE_FUNCTIONS_PATTERN_USERS_NOT_FRIENDS, 
                preferencesKey: Constants.PREFERENCES_NOT_FRIENDS_USERS, 
                converter: Converter.ToFriend);
        }

        public async void SendFriendRequest(string username)
        {
            dynamic request = new JObject
            {
                { "userName", username },
                { "requester", Preferences.Get(Constants.PREFERENCES_USERNAME, String.Empty) },
                { "requestDate", DateTime.Now.Date.ToShortDateString() }
            };
            string json = JsonConvert.SerializeObject(request);
            
            r_Logger.LogInformation($"request:{Environment.NewLine}{json}");
            
            createJsonStringContent(JsonConvert.SerializeObject(request));
            await postAsync(requestUri: Constants.AZURE_FUNCTIONS_PATTERN_FRIEND_REQUEST, createJsonStringContent(json));
        }

        public async Task<List<FriendRequest>> GetFriendRequests(string userName)
        {
            string requestUri, responseJson;
            HttpResponseMessage response;
            List<FriendRequest> friendRequests;
            
            try
            {
                requestUri = Constants.AZURE_FUNCTIONS_PATTERN_FRIEND_REQUEST + $"?username={userName}";
                r_Logger.LogInformation($"request URI {requestUri}");
                response = await m_HttpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                responseJson = await response.Content.ReadAsStringAsync();
                friendRequests = JsonConvert.DeserializeObject<List<FriendRequest>>(responseJson);
                r_Logger.LogInformation($"Got {friendRequests.Count} friend requests");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on GetFriendRequests: {Environment.NewLine}{ex.Message}");
                friendRequests = new List<FriendRequest>();
            }

            return friendRequests;
        }

        public async Task RejectFriendRequest(string userName, string requester)
        {
            HttpResponseMessage response;
            dynamic data = new JObject
            {
                { "requester", requester },
                { "userName", userName }
            };
            string json = JsonConvert.SerializeObject(data);

            r_Logger.LogInformation($"Reject friend request:{Environment.NewLine}{json}");

            response = await postAsync(
                requestUri: Constants.AZURE_FUNCTIONS_PATTERN_REJECT_FRIEND_REQUEST,
                content: createJsonStringContent(json));

            r_Logger.LogInformation($"Friend request from {requester} to {userName} was rejected");
        }

        public async Task AcceptFriendRequest(string userName, string requester)
        {
            HttpResponseMessage response;
            dynamic data = new JObject
            {
                { "requester", requester },
                { "userName", userName }
            };
            string json = JsonConvert.SerializeObject(data);

            r_Logger.LogInformation($"Accept friend request:{Environment.NewLine}{json}");

            response = await postAsync(
                requestUri: Constants.AZURE_FUNCTIONS_PATTERN_ACCEPT_FRIEND_REQUEST,
                content: createJsonStringContent(json));

            r_Logger.LogInformation($"Friend request from {requester} to {userName} was accepted");
        }
        
        public async Task<string> UploadProfilePictureToBLOB(string base64Image)
        {
            dynamic data = new JObject();
            dynamic responseObject;
            string json, responseBody;
            HttpResponseMessage response;
            string imageUrl = null;

            try
            {
                data.image = base64Image;

                json = JsonConvert.SerializeObject(data);
                r_Logger.LogInformation($"request:{Environment.NewLine}{data}");

                response = await postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_UPLOAD_PROFILE_PICTURE_TO_BLOB,
                    content: createJsonStringContent(json));

                response.EnsureSuccessStatusCode();
        
                responseBody = await response.Content.ReadAsStringAsync();
                responseObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                imageUrl = responseObject.imageUrl;
        
                r_Logger.LogInformation($"Successful status code from Azure Function from UploadProfilePicture.");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred on UploadProfilePicture: {Environment.NewLine}{ex.Message}");
            }

            return imageUrl;
        }
        
        public async Task UpdateUserProfilePictureAsync(string username, string imageUrl)
        {
            string json;
            dynamic userData = new JObject
            {
                { "userName", username },
                { "profilePicture", imageUrl }
            };
            
            HttpResponseMessage response;
            json = JsonConvert.SerializeObject(userData);

            r_Logger.LogInformation($"Updating user profile picture");

            try
            {
                response = await postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_UPDATE_USER,
                    content: createJsonStringContent(json));

                response.EnsureSuccessStatusCode();
                r_Logger.LogInformation($"Successfully updated profile picture for username: {userData.userName}");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred on UpdateUserProfilePictureAsync: {Environment.NewLine}{ex.Message}");
            }
        }
        
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            string requestUri, responseJson;
            HttpResponseMessage response;
            User user;

            try
            {
                requestUri = Constants.AZURE_FUNCTIONS_PATTERN_USER + $"/{username}";
                r_Logger.LogInformation($"Requesting user profile for username: {requestUri}");
        
                response = await m_HttpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                responseJson = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(responseJson);
        
                r_Logger.LogInformation($"Successfully retrieved user for username: {username}");
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occurred on GetUserByUsernameAsync: {Environment.NewLine}{ex.Message}");
                user = null;
            }

            return user;
        }
        
        public async Task<List<Location>> GetNearbyPlaces(string destination, Location location)
        {
            List<Location> nearbyPlaces = new List<Location>();
            HttpResponseMessage response;
            dynamic returnedObject;
            dynamic data = new JObject
            {
                { "type", destination },
                { "longitude", location.Longitude },
                { "latitude", location.Latitude }
            };

            try
            {
                response = await postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_DYNAMIC_DESTINATION,
                    content: createJsonStringContent(JsonConvert.SerializeObject(data)));

                response.EnsureSuccessStatusCode();
                LoggerService.Instance.LogDebug($"Successful status code from Azure Function from GetNearbyPlaces");

                returnedObject = await DeserializeObjectFromResponseAsync(response);
                if (returnedObject != null)
                {
                    foreach (dynamic place in returnedObject)
                    {
                        nearbyPlaces.Add(new Location(
                            name: (string)place.name,
                            address: (string)place.address,
                            longitude: (double)place.longitude,
                            latitude: (double)place.latitude));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.Instance.LogError($"Error occured on GetNearbyPlaces: {ex.Message}");
            }
            
            return nearbyPlaces;
        }

        public async Task<bool> DeleteNotificationAsync(string notificationID)
        {
            HttpResponseMessage response;
            bool isDeleted;
            string json;
            List<Notification> notifications;
            dynamic data = new JObject
            {
                { "id", notificationID }
            };
            
            try
            {
                response = await deleteAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION,
                    content: createJsonStringContent(JsonConvert.SerializeObject(data)));

                response.EnsureSuccessStatusCode();
                LoggerService.Instance.LogDebug($"Notification with id {notificationID} was deleted");
                isDeleted = true;

                json = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
                notifications = JsonConvert.DeserializeObject<List<Notification>>(json);
                notifications.RemoveAll(notification => notification.ID == notificationID);
                Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(notifications));
            }
            catch (Exception ex)
            {
                LoggerService.Instance.LogError($"Error occured on DeleteNotificationAsync: {ex.Message}");
                isDeleted = false;
            }
            
            return isDeleted;
        }
        
        private async Task<HttpResponseMessage> deleteAsync(string requestUri, StringContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = content
            };
            
            r_Logger.LogInformation($"request:{Environment.NewLine}{request}");

            return await m_HttpClient.SendAsync(request);
        }

        public async Task<bool> RenewNotificationAsync(string creator, string notificationID)
        {
            string json;
            bool isRenewed;
            HttpResponseMessage response;
            dynamic data = new JObject
            {
                { "creator", creator },
                { "id", notificationID }
            };

            try
            {
                r_Logger.LogInformation($"request:{Environment.NewLine}{data}");
                
                json = JsonConvert.SerializeObject(data);
                response = await postAsync(
                    requestUri: Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_RENEW, 
                    content: createJsonStringContent(json));
                response.EnsureSuccessStatusCode();
                r_Logger.LogDebug($"Successful status code from Azure Function from createNotification");
                isRenewed = true;
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on createNotification: {ex.Message}");
                isRenewed = false;
            }

            return isRenewed;
        }

        public async Task<bool> UpdateTimeNotificationAsync(string ID, string name, string description, string type, DateTime dateTime)
        {
            long timestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

            return await updateNotification(ID, name, description, type, "timestamp", timestamp, 
                Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_TIME);
        }
        
        public async Task<bool> UpdateLocationNotificationAsync(string ID, string name, string description, string type, string location, string activation, bool permanent)
        {
            return await updateNotification(ID, name, description, type, "location", location, Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_LOCATION, activation, permanent);
        }
        
        public async Task<bool> UpdateDynamicNotificationAsync(string ID, string name, string description, string type, string location)
        {
            return await updateNotification(ID, name, description, type, "location", location, Constants.AZURE_FUNCTIONS_PATTERN_NOTIFICATION_UPDATE_DYNAMIC);
        }
        
        private async Task<bool> updateNotification(string ID, string name, string description, string type, string key, JToken value, string requestUri, string activation = "", bool permanent = false)
        {
            string json;
            bool isUpdated;
            HttpResponseMessage response;
            dynamic data = new JObject
            {
                { "id", ID },
                { "name", name },
                { "description", description },
                { "type", type },
                { key, value }
            };
            
            if (type.Equals(Constants.LOCATION))
            {
                data.activation = activation;
                data.permanent = permanent;
            }

            try
            {
                json = JsonConvert.SerializeObject(data);
                r_Logger.LogInformation($"request:{Environment.NewLine}{json}");
                
                response = await postAsync(requestUri, createJsonStringContent(json));
                response.EnsureSuccessStatusCode();
                r_Logger.LogDebug($"Successful status code from Azure Function from updateNotification");
                isUpdated = true;
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on updateNotification: {ex.Message}");
                isUpdated = false;
            }

            return isUpdated;
        }
    }
}
