namespace Notify.Functions.Core
{
    public static class Constants
    {
        public static readonly string DATABASE_NOTIFY_MTA = "Notify_MTA";
        public static readonly string CONNECTION_STRING = @"mongodb://notify-mta-database:7TybNmpnXUblzkln3VjZqlX0t6MT9QekAoKnImhzLM0zBr5uphSVCjwzgBX6XjNRzVcnaSygAwVRACDbApzIsg==@notify-mta-database.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@notify-mta-database@";
        public static readonly string COLLECTION_DESTINATION = "Destination";
        public static readonly string COLLECTION_NOTIFICATION = "Notification";
        public static readonly string COLLECTION_USER = "User";
        public static readonly string COLLECTION_FRIEND = "Friend";
        public static readonly string COLLECTION_FRIEND_REQUEST = "Friend_Request";
        public static readonly string COLLECTION_GROUP = "Group";
    }
}
