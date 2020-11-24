using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public class DocumentServiceConfiguration : IDocumentServiceConfiguration
    {
        public DocumentServiceConfiguration(string endpointUrl)
        {
            Guard.AgainstNullOrEmptyString(endpointUrl, nameof(endpointUrl));
            
            EndpointUrl = endpointUrl;
        }

        public string EndpointUrl { get; }
    }
}