using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Notify.Functions.Core;

namespace Notify.Functions.NotifyFunctions.AzureVault
{
    public static class AzureVault
    {
        public static async Task<string> GetSecretFromVault(string secretName)
        {
            SecretClient client = new SecretClient(new Uri(Constants.AZURE_KEY_VAULT), new DefaultAzureCredential());
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);

            return secret.Value;
        }

        public static async Task<string> ProcessPasswordWithKeyVault(string password, string keyName, string operation)
        {
            string processedPassword;

            Uri keyVaultUri = new Uri(Constants.AZURE_KEY_VAULT);

            KeyClient keyClient = new KeyClient(keyVaultUri, new DefaultAzureCredential());
            KeyVaultKey keyVaultKey = await keyClient.GetKeyAsync(keyName);

            CryptographyClient cryptographyClient =
                new CryptographyClient(keyVaultKey.Id, new DefaultAzureCredential());

            if (operation.Equals(Constants.ENCRYPT_OPERATION))
            {
                byte[] unencryptedBytes = Encoding.UTF8.GetBytes(password);

                EncryptResult encryptResult =
                    await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, unencryptedBytes);

                processedPassword = Convert.ToBase64String(encryptResult.Ciphertext);
            }
            else
            {
                byte[] encryptedBytes = Convert.FromBase64String(password);

                DecryptResult decryptResult =
                    await cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, encryptedBytes);

                processedPassword = Encoding.UTF8.GetString(decryptResult.Plaintext);
            }

            return processedPassword;
        }
    }
}
