using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public ContentStoreBuilder AddContentStore()
        {
            services.AddOptions<ContentStoreOptions>().ValidateOnStart();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ContentStoreOptions>, ContentStoreOptionsValidator>());
            services.TryAddSingleton<IContentStoreProvider, ContentStoreProvider>();
            services.TryAddSingleton<IContentStore>(serviceProvider => serviceProvider.GetRequiredService<IContentStoreProvider>().Default);

            return new(services);
        }
    }
}
