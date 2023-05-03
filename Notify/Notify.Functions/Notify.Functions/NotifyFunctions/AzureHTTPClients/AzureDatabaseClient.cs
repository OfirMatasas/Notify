using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using MongoDB.Driver;
using Azure.Identity;
using Notify.Functions.Core;

namespace Notify.Functions.NotifyFunctions.AzureHTTPClients
{
    public sealed class AzureDatabaseClient
    {
        private static AzureDatabaseClient m_Instance;
        private static MongoClient m_MongoClient;
        private static readonly object m_LockInstance = new object();
        
        private AzureDatabaseClient()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(getDatabaseConnectionString().Result));
            m_MongoClient = new MongoClient(settings);
        }

        public static AzureDatabaseClient Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (m_LockInstance)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new AzureDatabaseClient();
                        }
                    }
                }

                return m_Instance;
            }
        }
        
        public IMongoDatabase GetDatabase(string databaseName)
        {
            return m_MongoClient.GetDatabase(databaseName);
        }
        
        public IMongoCollection<T> GetCollection<T>(string databaseName, string collectionName)
        {
            return GetDatabase(databaseName).GetCollection<T>(collectionName);
        }

        private async Task<string> getDatabaseConnectionString()
        {
            SecretClient client = new SecretClient(new Uri(Constants.AZURE_KEY_VAULT), new DefaultAzureCredential());
            KeyVaultSecret secret = await client.GetSecretAsync("TWILIO-ACCOUNT-SID");
            Console.WriteLine(secret.Value);
            return secret.Value;
        }
    }
}
