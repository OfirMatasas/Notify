using Twilio.Rest.Api.V2010.Account.AvailablePhoneNumberCountry;

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
        public static readonly string AZURE_BLOB_CONTAINER_NAME = "notifycontainer";
        public static readonly string BLOB_DEFAULT_PROFILE_IMAGE = "https://notifyblobstorage.blob.core.windows.net/notifycontainer/4199b89b-48e4-477c-9496-50c340ccd882.jpg";

        #region AzureSecretsAndKeys
        
        public static readonly string AZURE_KEY_VAULT = "https://notify-keys-vault.vault.azure.net/";
        public static readonly string GOOGLE_API_KEY = "GOOGLE-API-KEY";
        public static readonly string DATABASE_CONNECTION_STRING = "MONGO-CONNECTION-STRING";
        public static readonly string TWILIO_ACCOUNT_SID = "TWILIO-ACCOUNT-SID";
        public static readonly string TWILIO_AUTH_TOKEN = "TWILIO-AUTH-TOKEN";
        public static readonly string TWILIO_PHONE_NUMBER = "TWILIO-PHONE-NUMBER";
        public static readonly string PASSWORD_ENCRYPTION_KEY = "NotifyPasswordEncryptionKey";
        public static readonly string AZURE_BLOB_CONNECTION_STRING = "AZURE-BLOB-CONNECTION-STRING";

        #endregion

        #region GoogleHttpClient

        public static readonly string GOOGLE_API_BASE_URL = "https://maps.googleapis.com/maps/api/";
        public static readonly int GOOGLE_API_RADIUS = 1000;

        #endregion
    }
}
