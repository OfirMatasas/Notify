using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Notify.Functions.Core;

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
        Uri keyVaultUri = new Uri(Constants.AZURE_KEY_VAULT);

        KeyClient keyClient = new KeyClient(keyVaultUri, new DefaultAzureCredential());
        KeyVaultKey keyVaultKey = await keyClient.GetKeyAsync(keyName);

        CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKey.Id, new DefaultAzureCredential());

        byte[] unencryptedBytes = Encoding.UTF8.GetBytes(unencryptedPassword);

        EncryptResult encryptResult =
            await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, unencryptedBytes);

        return Convert.ToBase64String(encryptResult.Ciphertext);
    }

    public static async Task<string> DecryptPasswordWithKeyVault(string encryptedPassword, string keyName)
    {
        Uri keyVaultUri = new Uri(Constants.AZURE_KEY_VAULT);

        KeyClient keyClient = new KeyClient(keyVaultUri, new DefaultAzureCredential());
        KeyVaultKey keyVaultKey = await keyClient.GetKeyAsync(keyName);

        CryptographyClient cryptographyClient = new CryptographyClient(keyVaultKey.Id, new DefaultAzureCredential());

        byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);

        DecryptResult decryptResult =
            await cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, encryptedBytes);

        return Encoding.UTF8.GetString(decryptResult.Plaintext);
    }
}
