using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Opswat
{
    public class OpswatApi : IOpswatApi
    {
        private readonly string _apiUrl;
        private readonly IRestClient _client;

        public OpswatApi(IOpswatConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _apiUrl = configuration.ApiUrl;

            if (!_apiUrl.EndsWith("/"))
            {
                _apiUrl += "/";
            }

            _client = new RestClient(configuration.ApiUrl);

            _client.AddDefaultHeader("apikey", configuration.ApiKey);

            _client.UseNewtonsoftJson();
        }

        public T Get<T>(RestRequest request) where T : new()
        {
            Guard.AgainstNull(request, nameof(request));

            var response = _client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            return response.Data;
        }

        public IRestResponse GetResponse(RestRequest request) 
        {
            Guard.AgainstNull(request, nameof(request));

            var response = _client.Execute(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            return response;
        }

        public string GetFullApiUrl(string path)
        {
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            return path.StartsWith("/")
                ? $"{_apiUrl}{path.Substring(1)}"
                : $"{_apiUrl}{path}";
        }
    }
}