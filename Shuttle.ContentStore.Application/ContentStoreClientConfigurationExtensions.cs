using System;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public static class ContentStoreClientConfigurationExtensions
    {
        public static Uri GetApiUrl(this IContentStoreClientConfiguration configuration, string path)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            return new Uri(configuration.Url, path);
        }
    }
}