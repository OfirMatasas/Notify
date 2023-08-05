using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Notify.Functions.NotifyFunctions.Users
{
    public static class UploadProfilePicture
    {
        [FunctionName("UploadProfilePicture")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "uploadProfilePicture")]
            HttpRequest req, ILogger log)
        {
            string requestBody;
            dynamic data;
            string base64Image;
            ObjectResult result;
            Stream imageStream;
            string filename;
            string imageUrl;

            log.LogInformation("Got client's HTTP request to upload profile picture to BLOB storage");

            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
                log.LogInformation($"Data:{Environment.NewLine}{data}");

                base64Image = Convert.ToString(data.image);
                if (string.IsNullOrEmpty(base64Image))
                {
                    return new BadRequestObjectResult("Image is required.");
                }

                byte[] imageBytes = Convert.FromBase64String(base64Image);
                imageStream = new MemoryStream(imageBytes);

                filename = Guid.NewGuid() + ".jpg";

                imageUrl = await AzureBlob.AzureBlob.UploadImageToBlobStorage(imageStream, filename);

                log.LogInformation($"Profile picture uploaded successfully to {imageUrl}");

                result = new OkObjectResult(new { imageUrl });
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to upload profile picture. Reason: {ex.Message}");
                result = new ObjectResult($"Failed to upload profile picture.{Environment.NewLine}Error: {ex.Message}");
            }

            return result;
        }
    }
}

