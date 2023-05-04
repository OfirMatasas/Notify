using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Notify.Functions.Core;

namespace Notify.Functions.NotifyFunctions.AzureVault;

public sealed class AzureVault
{
    public static async Task<string> GetSecretFromVault(string secretName)
    {
        SecretClient client = new SecretClient(new Uri(Constants.AZURE_KEY_VAULT), new DefaultAzureCredential());
        KeyVaultSecret secret = await client.GetSecretAsync(secretName);
        return secret.Value;
    }
}