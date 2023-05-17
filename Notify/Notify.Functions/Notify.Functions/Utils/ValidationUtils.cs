using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Notify.Functions.Utils
{
    public static class ValidationUtils
    {
        public static bool ValidateUserName(HttpRequest req, ILogger log)
        {
            bool valid = false;

            if (string.IsNullOrEmpty(req.Query["username"]))
            {
                log.LogError("The 'username' query parameter is required");
            }
            else
            {
                valid = true;
            }

            return valid;
        }
    }
}
