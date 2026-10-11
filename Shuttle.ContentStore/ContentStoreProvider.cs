using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore;

public class ContentStoreProvider(IServiceProvider serviceProvider, IOptions<ContentStoreOptions> contentStoreOptions) : IContentStoreProvider
{
    public IEnumerable<string> Names => contentStoreOptions.Value.StoreNames.AsReadOnly();

    public IContentStore Default => Get(GetDefaultName());

    public bool Contains(string name)
    {
        return contentStoreOptions.Value.StoreNames.Contains(Guard.AgainstEmpty(name), StringComparer.Ordinal);
    }

    public IContentStore Get(string name)
    {
        if (!Contains(name))
        {
            throw new InvalidOperationException($"No content store named '{name}' has been registered.  Registered stores: {FormatNames()}.");
        }

        return serviceProvider.GetRequiredKeyedService<IContentStore>(name);
    }

    private string GetDefaultName()
    {
        var options = contentStoreOptions.Value;

        if (!string.IsNullOrWhiteSpace(options.DefaultStore))
        {
            return options.DefaultStore;
        }

        return options.StoreNames.Count switch
        {
            0 => throw new InvalidOperationException("No content stores have been registered."),
            1 => options.StoreNames[0],
            _ => throw new InvalidOperationException($"Multiple content stores have been registered ({FormatNames()}) but no default has been specified.  Call 'SetDefault' on the content store builder or set '{nameof(ContentStoreOptions.DefaultStore)}'.")
        };
    }

    private string FormatNames()
    {
        var names = contentStoreOptions.Value.StoreNames;

        return names.Count == 0 ? "(none)" : string.Join(", ", names.Select(name => $"'{name}'"));
    }
}
