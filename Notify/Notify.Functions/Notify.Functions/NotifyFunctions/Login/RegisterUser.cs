using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureHTTPClients;
using MongoUtils = Notify.Functions.Utils.MongoUtils;

namespace Notify.Functions.NotifyFunctions.Login
{
    public static class UserRegistration
    {
        [FunctionName("UserRegistration")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")]
            HttpRequest req, ILogger log)
        {
            IMongoCollection<BsonDocument> collection;
            string requestBody;
            dynamic data;
            ObjectResult result;
            string encryptedPassword;

            log.LogInformation("Got client's HTTP request to register");

            try
            {
                collection = AzureDatabaseClient.Instance.GetCollection<BsonDocument>(Constants.DATABASE_NOTIFY_MTA,
                    Constants.COLLECTION_USER);
                log.LogInformation(
                    $"Got reference to {Constants.COLLECTION_USER} collection on {Constants.DATABASE_NOTIFY_MTA} database");

                await MongoUtils.CreatePropertyIndexes(collection, "userName", "telephone");

                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
                log.LogInformation($"Data:{Environment.NewLine}{data}");
                
                data.password = await EncryptPassword(Convert.ToString(data.password));
                
                log.LogInformation($"User password encrypted successfully: {data.password}");

                BsonDocument userDocument = new BsonDocument
                {
                    { "name", Convert.ToString(data.name) },
                    { "userName", Convert.ToString(data.userName) },
                    { "password", Convert.ToString(data.password) },
                    { "telephone", Convert.ToString(data.telephone) }
                };
                
                log.LogInformation($"New data:{Environment.NewLine}{data}");

                await collection.InsertOneAsync(userDocument);
                log.LogInformation(
                    $"Inserted user with username {data.userName} and telephone {data.telephone} into database");

                result = new OkObjectResult(JsonConvert.SerializeObject(data));
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to insert user. Reason: {ex.Message}");
                result = new ObjectResult($"Failed to register.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }
        
        private static async Task<string> EncryptPassword(string unencryptedPassword)
        {
            string encryptionKey, encryptedPassword = null;

            try
            {
                encryptionKey = await AzureVault.AzureVault.GetSecretFromVault(Constants.PASSWORD_ENCRYPTION_KEY);
                Debug.WriteLine("Encryption key retrieved from Azure Key Vault.");

                using (Aes aes = Aes.Create())
                {
                    // Set the encryption key and mode
                    aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                    aes.Mode = CipherMode.CBC;
                    Debug.WriteLine("Encryption key and mode set.");

                    // Generate a random IV (Initialization Vector)
                    aes.GenerateIV();
                    Debug.WriteLine("Initialization Vector (IV) generated.");

                    // Convert the unencrypted password to bytes
                    byte[] unencryptedBytes = Encoding.UTF8.GetBytes(unencryptedPassword);
                    Debug.WriteLine("Unencrypted password converted to bytes.");

                    // Perform encryption
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(unencryptedBytes, 0, unencryptedBytes.Length);
                        encryptedPassword = Convert.ToBase64String(encryptedBytes);
                        Debug.WriteLine("Password encrypted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to encrypt the password.{Environment.NewLine}{ex.Message}");
            }

            return encryptedPassword;
        }
    }
}
