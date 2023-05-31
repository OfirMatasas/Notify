namespace Notify.Functions.Core
{
    public static class Constants
    {
        public static readonly string DATABASE_NOTIFY_MTA = "Notify_MTA";
        public static readonly string COLLECTION_DESTINATION = "Destination";
        public static readonly string COLLECTION_NOTIFICATION = "Notification";
        public static readonly string COLLECTION_USER = "User";
        public static readonly string COLLECTION_FRIEND = "Friend";
        public static readonly string COLLECTION_FRIEND_REQUEST = "Friend_Request";
        public static readonly string COLLECTION_GROUP = "Group";
        public static readonly string ENCRYPT_OPERATION = "encrypt";
        public static readonly string DECRYPT_OPERATION = "decrypt";

        #region AzureSecretsAndKeys
        
        public static readonly string AZURE_KEY_VAULT = "https://notify-keys-vault.vault.azure.net/";
        public static readonly string GOOGLE_API_KEY = "GOOGLE-API-KEY";
        public static readonly string DATABASE_CONNECTION_STRING = "MONGO-CONNECTION-STRING";
        public static readonly string TWILIO_ACCOUNT_SID = "TWILIO-ACCOUNT-SID";
        public static readonly string TWILIO_AUTH_TOKEN = "TWILIO-AUTH-TOKEN";
        public static readonly string TWILIO_PHONE_NUMBER = "TWILIO-PHONE-NUMBER";
        public static readonly string PASSWORD_ENCRYPTION_KEY = "NotifyPasswordEncryptionKey";

        #endregion

        #region GoogleHttpClient

        public static readonly string GOOGLE_API_BASE_URL = "https://maps.googleapis.com/maps/api/";

        #endregion
    }
}
