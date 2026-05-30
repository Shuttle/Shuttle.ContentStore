namespace Shuttle.ContentStore;

public interface IContentStore
{
    string Name { get; }
    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);
    Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}