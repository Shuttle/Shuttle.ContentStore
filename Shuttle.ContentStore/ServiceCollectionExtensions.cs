using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.ContentStore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public ContentStoreBuilder AddContentStore()
        {
            return new ContentStoreBuilder(services);
        }
    }
}