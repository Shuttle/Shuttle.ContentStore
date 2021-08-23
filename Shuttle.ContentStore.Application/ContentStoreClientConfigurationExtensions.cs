using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Application
{
    public static class ContentStoreClientConfigurationExtensions
    {
        public static string GetEndpoint(this IContentStoreClientConfiguration configuration, string path)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            return $"{configuration.Url}{(configuration.Url.EndsWith("/") ? string.Empty : "/")}{(path.StartsWith("/") ? path.Substring(1) : path)}";
        }
    }
}