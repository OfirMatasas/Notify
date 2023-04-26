using MongoDB.Driver;
using Notify.Functions.Core;

namespace Notify.Functions.NotifyFunctions.AzureHTTPClients
{
    public sealed class AzureDatabaseClient
    {
        private static MongoClient mongoClient;
        private static object lockInstance = new object();
        private static AzureDatabaseClient client;
        
        private AzureDatabaseClient()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Constants.DATABASE_CONNECTION_STRING));
            mongoClient = new MongoClient(settings);
        }

        public static AzureDatabaseClient Instance
        {
            get
            {
                if (client == null)
                {
                    lock (lockInstance)
                    {
                        if (client == null)
                        {
                            client = new AzureDatabaseClient();
                        }
                    }
                }

                return client;
            }
        }
        
        public IMongoDatabase GetDatabase(string databaseName)
        {
            return mongoClient.GetDatabase(databaseName);
        }
    }
}
