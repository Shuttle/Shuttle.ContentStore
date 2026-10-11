namespace Shuttle.ContentStore;

public interface IContentStoreProvider
{
    IEnumerable<string> Names { get; }
    IContentStore Default { get; }
    bool Contains(string name);
    IContentStore Get(string name);
}
