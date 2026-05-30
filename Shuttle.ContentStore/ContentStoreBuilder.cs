using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.ContentStore;

public class ContentStoreBuilder(ServiceCollection services)
{
    public ServiceCollection Services { get; } = services;

    private void RemoveContentStore()
    {
        var descriptor = Services.FirstOrDefault(serviceDescriptor => 
            serviceDescriptor.ServiceType == typeof(IContentStore) && !serviceDescriptor.IsKeyedService);

        if (descriptor == null)
        {
            return;
        }

        Services.Remove(descriptor);
    }

    public ContentStoreBuilder AddContentStore<T>() where T : IContentStore
    {
        RemoveContentStore();

        Services.AddSingleton(typeof(IContentStore), typeof(T));
        Services.AddKeyedSingleton(typeof(IContentStore), "azure", typeof(T));

        return this;
    }

    public ContentStoreBuilder SetDefault(string key)
    {
        Guard.AgainstEmpty(key);

        RemoveContentStore();

        Services.AddSingleton<IContentStore>(serviceProvider => serviceProvider.GetRequiredKeyedService<IContentStore>(key));

        return this;
    }
}