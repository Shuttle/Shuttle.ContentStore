using Shuttle.Contract;

namespace Shuttle.ContentStore.FileSystem;

public class FileSystemContentStore(string name, FileSystemContentStoreOptions fileSystemContentStoreOptions) : IContentStore
{
    private const int BufferSize = 81920;

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private readonly string _rootPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Guard.AgainstEmpty(Guard.AgainstNull(fileSystemContentStoreOptions).RootPath), AppContext.BaseDirectory));

    public string Name { get; } = Guard.AgainstEmpty(name);

    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous));
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new FileNotFoundException($"Content not found for key '{key}' in store '{Name}'.", key, ex);
        }
    }

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);

        Guard.AgainstNull(content);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Write to a temporary file first so that readers never observe partially written content.
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous))
            {
                await content.CopyToAsync(stream, cancellationToken);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        catch
        {
            File.Delete(temporaryPath);
            throw;
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(File.Exists(path));
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);

        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string GetPath(string key)
    {
        Guard.AgainstEmpty(key);

        var path = Path.GetFullPath(Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar)));

        if (!path.StartsWith(_rootPath + Path.DirectorySeparatorChar, PathComparison))
        {
            throw new ArgumentException($"Key '{key}' resolves to a path outside of the root path of store '{Name}'.", nameof(key));
        }

        return path;
    }
}
