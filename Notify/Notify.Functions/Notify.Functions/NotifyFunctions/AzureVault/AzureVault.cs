using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Notify.Functions.Core;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Azure.Cosmos.Linq;

namespace Notify.Functions.NotifyFunctions.AzureVault;

public static class AzureVault
{
    public static async Task<string> GetSecretFromVault(string secretName)
    {
        SecretClient client = new SecretClient(new Uri(Constants.AZURE_KEY_VAULT), new DefaultAzureCredential());
        KeyVaultSecret secret = await client.GetSecretAsync(secretName);

        return secret.Value;
    }
    
    public static async Task<string> EncryptPasswordWithKeyVault(string unencryptedPassword, string keyName)
    {
        // Create a Uri for the Azure Key Vault
        Uri keyVaultUri = new Uri(Constants.AZURE_KEY_VAULT);

        // Get a reference to the key in the key vault
        KeyClient keyClient = new KeyClient(keyVaultUri, new DefaultAzureCredential());
        KeyVaultKey keyVaultKey = await keyClient.GetKeyAsync(keyName);

        // Create a cryptographic client that will use the key from the key vault
        CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKey.Id, new DefaultAzureCredential());

        // Convert the unencrypted password to bytes
        byte[] unencryptedBytes = Encoding.UTF8.GetBytes(unencryptedPassword);

        // Encrypt the data using the cryptographic client
        EncryptResult encryptResult = await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, unencryptedBytes);

        // Convert the encrypted data to a string so it can be returned
        return Convert.ToBase64String(encryptResult.Ciphertext);
    }
}