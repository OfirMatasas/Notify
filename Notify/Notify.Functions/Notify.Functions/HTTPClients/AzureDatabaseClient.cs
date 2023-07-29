using MongoDB.Driver;
using Notify.Functions.Core;
using Notify.Functions.NotifyFunctions.AzureVault;

namespace Notify.Functions.HTTPClients
{
    public sealed class AzureDatabaseClient
    {
        private static AzureDatabaseClient m_Instance;
        private static MongoClient m_MongoClient;
        private static readonly object m_LockInstance = new object();
        
        private AzureDatabaseClient()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(AzureVault.GetSecretFromVault(Constants.DATABASE_CONNECTION_STRING).Result));
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
    }
}
