using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public class ContentStoreClientConfiguration : IContentStoreClientConfiguration
    {
        public ContentStoreClientConfiguration(string apiUrl)
        {
            Guard.AgainstNullOrEmptyString(apiUrl, nameof(apiUrl));
            
            Url = apiUrl;
        }

        public string Url { get; }
    }
}