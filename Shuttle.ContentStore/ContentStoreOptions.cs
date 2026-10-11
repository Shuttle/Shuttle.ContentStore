namespace Shuttle.ContentStore;

public class ContentStoreOptions
{
    public const string SectionName = "Shuttle:ContentStore";

    /// <summary>
    /// The name of the store resolved for an unkeyed <see cref="IContentStore"/>.  When empty, and exactly one store
    /// has been registered, that store is used.
    /// </summary>
    public string DefaultStore { get; set; } = string.Empty;

    /// <summary>
    /// The names of all registered stores.  Populated by the <see cref="ContentStoreBuilder"/>.
    /// </summary>
    public List<string> StoreNames { get; } = [];
}
