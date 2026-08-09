using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.ContentStore;

public class ContentStoreBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    private void RemoveContentStore(string? key = null)
    {
        var descriptor = Services.FirstOrDefault(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(IContentStore) &&
            (key == null
                ? !serviceDescriptor.IsKeyedService
                : serviceDescriptor.IsKeyedService && Equals(serviceDescriptor.ServiceKey, key)));

        if (descriptor == null)
        {
            return;
        }

        Services.Remove(descriptor);
    }

    public ContentStoreBuilder AddContentStore<T>(string key) where T : IContentStore
    {
        Guard.AgainstEmpty(key);

        RemoveContentStore();
        RemoveContentStore(key);

        Services.AddSingleton(typeof(IContentStore), typeof(T));
        Services.AddKeyedSingleton(typeof(IContentStore), key, typeof(T));

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