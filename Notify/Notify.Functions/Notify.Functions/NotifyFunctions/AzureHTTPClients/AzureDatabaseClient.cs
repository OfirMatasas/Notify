using System;
using MongoDB.Driver;
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
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Constants.CONNECTION_STRING));
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
    }
}
