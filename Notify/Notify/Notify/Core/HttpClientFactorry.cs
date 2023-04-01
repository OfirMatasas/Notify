namespace Notify.Core
{
    public class HttpClientFactory
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public HttpClientFactory()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }

        public System.Net.Http.HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}