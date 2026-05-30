using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.ContentStore;

public static class ServiceCollectionExtensions
{
    extension(ServiceCollection services)
    {
        public ContentStoreBuilder AddContentStore()
        {
            return new ContentStoreBuilder(services);
        }
    }
}