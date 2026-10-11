using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.ContentStore;

public class ContentStoreBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Registers a store with the given logical name.  Store names must be unique across all providers.
    /// </summary>
    /// <param name="name">The logical name of the store, which is also the keyed service key.</param>
    /// <param name="factory">Creates the store; receives the service provider and the store name.</param>
    public ContentStoreBuilder AddContentStore(string name, Func<IServiceProvider, string, IContentStore> factory)
    {
        Guard.AgainstEmpty(name);
        Guard.AgainstNull(factory);

        if (Services.Any(descriptor => descriptor.ServiceType == typeof(IContentStore) && descriptor.IsKeyedService && Equals(descriptor.ServiceKey, name)))
        {
            throw new InvalidOperationException($"A content store named '{name}' has already been registered.");
        }

        Services.AddKeyedSingleton<IContentStore>(name, (serviceProvider, key) => factory(serviceProvider, (string)key!));
        Services.Configure<ContentStoreOptions>(options => options.StoreNames.Add(name));

        return this;
    }

    public ContentStoreBuilder SetDefault(string name)
    {
        Guard.AgainstEmpty(name);

        Services.Configure<ContentStoreOptions>(options => options.DefaultStore = name);

        return this;
    }
}
