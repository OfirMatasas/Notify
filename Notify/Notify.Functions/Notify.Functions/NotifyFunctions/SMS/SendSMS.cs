using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Notify.Functions.Core;
using Notify.Functions.Utils;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Notify.Functions.NotifyFunctions.SMS
{
    public static class SendSMS
    {
        [FunctionName("SendSMS")]
        [AllowAnonymous]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SendSMS")]
            HttpRequest request, ILogger logger)
        {
            dynamic data;
            string telephoneNumber, verificationCode;
            bool successfulSMSSend;
            ObjectResult result;

            logger.LogInformation("Got client's HTTP request to send SMS");

            data = await ConversionUtils.ExtractBodyContentAsync(request);
            logger.LogInformation($"Data:{Environment.NewLine}{data}");

            try
            {
                telephoneNumber = data.telephone;
                verificationCode = data.verificationCode;

                successfulSMSSend = await sendSMSVerificationCode(telephoneNumber, verificationCode, logger);

                if (successfulSMSSend)
                {
                    result = new OkObjectResult(
                        $"SMS sent to {telephoneNumber}{Environment.NewLine}Verification code: {verificationCode}");
                }
                else
                {
                    result = new BadRequestObjectResult($"Failed to send SMS message to {telephoneNumber}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to send SMS message. {ex.Message}");
                result = new BadRequestObjectResult("Failed to send SMS message");
            }

            return result;
        }

        private static async Task<bool> sendSMSVerificationCode(string telephoneNumber, string verificationCode,
            ILogger log)
        {
            string accountSid;
            string authToken;
            string twilioPhoneNumber;
            CreateMessageOptions messageOptions;
            MessageResource message;
            bool successfulSend = false;

            try
            {
                accountSid = await AzureVault.AzureVault.GetSecretFromVault(Constants.TWILIO_ACCOUNT_SID);
                authToken = await AzureVault.AzureVault.GetSecretFromVault(Constants.TWILIO_AUTH_TOKEN);
                twilioPhoneNumber = await AzureVault.AzureVault.GetSecretFromVault(Constants.TWILIO_PHONE_NUMBER);

                log.LogInformation($"Twilio Account SID: {accountSid}, Twilio Auth Token: {authToken}," +
                                   $" Twilio Phone Number: {twilioPhoneNumber}");

                TwilioClient.Init(accountSid, authToken);

                messageOptions = new CreateMessageOptions(new PhoneNumber(telephoneNumber))
                {
                    From = new PhoneNumber(twilioPhoneNumber),
                    Body = $"Your Notify verification code: {verificationCode}"
                };

                message = await MessageResource.CreateAsync(messageOptions);

                log.LogInformation(
                    $"SMS sent successfully to {message.To}.{Environment.NewLine}Message content: {message.Body}");

                successfulSend = true;
            }
            catch (Exception ex)
            {
                log.LogError(
                    $"Failed to send SMS message to {telephoneNumber}.{Environment.NewLine}Error: {ex.Message}");
            }

            return successfulSend;
        }
    }
}
