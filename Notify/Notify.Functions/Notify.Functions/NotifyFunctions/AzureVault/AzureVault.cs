using System;
using System.Diagnostics;
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
        try
        {
            return await ProcessPasswordWithKeyVault(unencryptedPassword, keyName,
                async (cryptographyClient, dataBytes) =>
                {
                    EncryptResult encryptResult =
                        await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, dataBytes);
                    
                    return Convert.ToBase64String(encryptResult.Ciphertext);
                });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error encrypting password: {ex.Message}");
            throw;
        }
    }

    public static async Task<string> DecryptPasswordWithKeyVault(string encryptedPassword, string keyName)
    {
        try
        {
            return await ProcessPasswordWithKeyVault(encryptedPassword, keyName,
                async (cryptographyClient, dataBytes) =>
                {
                    DecryptResult decryptResult =
                        await cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, dataBytes);
                    
                    return Encoding.UTF8.GetString(decryptResult.Plaintext);
                });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error decrypting password: {ex.Message}");
            throw;
        }
    }

    private static async Task<string> ProcessPasswordWithKeyVault(string password, string keyName,
        Func<CryptographyClient, byte[], Task<string>> processFunction)
    {
        try
        {
            Uri keyVaultUri = new Uri(Constants.AZURE_KEY_VAULT);
            KeyClient keyClient = new KeyClient(keyVaultUri, new DefaultAzureCredential());
            KeyVaultKey keyVaultKey = await keyClient.GetKeyAsync(keyName);
            CryptographyClient cryptographyClient =
                new CryptographyClient(keyVaultKey.Id, new DefaultAzureCredential());

            byte[] dataBytes = Encoding.UTF8.GetBytes(password);
            
            return await processFunction(cryptographyClient, dataBytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing password with Key Vault: {ex.Message}");
            throw;
        }
    }
}
