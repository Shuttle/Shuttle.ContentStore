using System;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public class ContentStoreClientConfiguration : IContentStoreClientConfiguration
    {
        public Uri Url { get; }

        public ContentStoreClientConfiguration(string url)
        {
            Guard.AgainstNullOrEmptyString(url, nameof(url));

            Url = new Uri($"{url}{(url.EndsWith("/") ? string.Empty : "/")}");
        }
    }
}