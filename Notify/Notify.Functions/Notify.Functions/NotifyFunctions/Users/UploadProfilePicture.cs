using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notify.Functions.Utils;

namespace Notify.Functions.NotifyFunctions.Users
{
    public static class UploadProfilePicture
    {
        [FunctionName("UploadProfilePicture")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "uploadProfilePicture")]
            HttpRequest request, ILogger logger)
        {
            string base64Image, fileName, imageUrl;
            dynamic data;
            ObjectResult result;
            Stream imageStream;
            byte[] imageBytes;

            logger.LogInformation("Got client's HTTP request to upload profile picture to BLOB storage");

            try
            {
                data = await ConversionUtils.ExtractBodyContentAsync(request);
                logger.LogInformation($"Data:{Environment.NewLine}{data}");

                base64Image = Convert.ToString(data.image);
                if (string.IsNullOrEmpty(base64Image))
                {
                    return new BadRequestObjectResult("Image is required.");
                }

                imageBytes = Convert.FromBase64String(base64Image);
                imageStream = new MemoryStream(imageBytes);

                fileName = $"{Guid.NewGuid()}.jpg";

                imageUrl = await AzureBlob.AzureBlob.UploadImageToBlobStorage(imageStream, fileName);

                logger.LogInformation($"Profile picture uploaded successfully to {imageUrl}");

                result = new OkObjectResult(new { imageUrl });
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to upload profile picture. Reason: {ex.Message}");
                result = new ObjectResult($"Failed to upload profile picture.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }
    }
}
