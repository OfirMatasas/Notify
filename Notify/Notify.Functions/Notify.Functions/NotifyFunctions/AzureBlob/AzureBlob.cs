using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Notify.Functions.Core;

namespace Notify.Functions.NotifyFunctions.AzureBlob;

public class AzureBlob
{
    public static async Task<string> UploadImageToBlobStorage(Stream imageStream, string filename)
    {
        CloudStorageAccount account = CloudStorageAccount.Parse(AzureVault.AzureVault.GetSecretFromVault(Constants.AZURE_BLOB_CONNECTION_STRING).Result);
        CloudBlobClient client = account.CreateCloudBlobClient();
        CloudBlobContainer container = client.GetContainerReference(Constants.AZURE_BLOB_CONTAINER_NAME);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

        await blockBlob.UploadFromStreamAsync(imageStream);

        return blockBlob.Uri.AbsoluteUri;
    }
}