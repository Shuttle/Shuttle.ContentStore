using System;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public class ContentStoreClient : IContentStoreClient
    {
        private readonly string _endpoint;
        private readonly IRestClient _client;

        public ContentStoreClient(IContentStoreClientConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _endpoint = configuration.GetEndpoint("/contents");

            _client = new RestClient(configuration.Url);

            _client.UseNewtonsoftJson();
        }

        public Guid Register(Guid referenceId, string fileName, string contentType, byte[] bytes,
            string systemName, string username, DateTime effectiveFromDate)
        {
            var request = new RestRequest(_endpoint)
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddFile("formFile", bytes, fileName, contentType);
            request.AddHeader("Content-Type", "multipart/form-data");

            request.AddParameter("id", referenceId, ParameterType.GetOrPost);
            request.AddParameter("contentType", contentType, ParameterType.GetOrPost);
            request.AddParameter("systemName", systemName, ParameterType.GetOrPost);
            request.AddParameter("username", username, ParameterType.GetOrPost);
            request.AddParameter("effectiveFromDate", $"{effectiveFromDate:O}", ParameterType.GetOrPost);

            var response = _client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException($"[{response.StatusDescription}] : {response.Content}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(response.Content);

            var contentIdValue = result.contentId.ToString();

            if (!Guid.TryParse(contentIdValue, out Guid contentId))
            {
                throw new ApplicationException($"Could not determine a valid 'contentId' result from the web-api response.  Value received was '{contentIdValue}'.");
            }

            return contentId;
        }
    }
}