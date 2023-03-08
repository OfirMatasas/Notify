using System.Net.Http;

namespace Notify.Core
{
    public class HttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactory()
        {
            _httpClient = new HttpClient();
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}