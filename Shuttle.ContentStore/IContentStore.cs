namespace Shuttle.ContentStore;

public interface IContentStore
{
    /// <summary>
    /// The logical name of the store (e.g. "documents"), which is also the key used to register the store as a keyed service.
    /// </summary>
    string Name { get; }

    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);
    Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
