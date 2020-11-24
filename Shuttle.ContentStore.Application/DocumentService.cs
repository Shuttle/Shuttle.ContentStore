using System;
using Newtonsoft.Json;
using RestSharp;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public class DocumentService : IDocumentService
    {
        private readonly string _endpoint;
        private readonly IRestClient _client;

        public DocumentService(IDocumentServiceConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _endpoint = configuration.EndpointUrl;

            _client = new RestClient(configuration.EndpointUrl);
        }

        public Guid Register(Guid referenceId, string fileName, string contentType, byte[] content,
            string systemName, string username, DateTime effectiveFromDate)
        {
            var request = new RestRequest(_endpoint)
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddFile("document", content, fileName, contentType);
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

            var documentIdValue = result.documentId.ToString();

            if (!Guid.TryParse(documentIdValue, out Guid documentId))
            {
                throw new ApplicationException($"Could not determine a valid 'documentId' result from the web-api response.  Value received was '{documentIdValue}'.");
            }

            return documentId;
        }
    }
}