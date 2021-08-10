using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public class ContentServiceConfiguration : IContentServiceConfiguration
    {
        public ContentServiceConfiguration(string apiUrl)
        {
            Guard.AgainstNullOrEmptyString(apiUrl, nameof(apiUrl));
            
            ApiUrl = apiUrl;
        }

        public string ApiUrl { get; }
    }
}