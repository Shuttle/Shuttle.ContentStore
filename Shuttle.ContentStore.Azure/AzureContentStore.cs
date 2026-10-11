using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Shuttle.Contract;

namespace Shuttle.ContentStore.Azure;

public class AzureContentStore(string name, AzureContentStoreOptions azureContentStoreOptions, BlobContainerClient blobContainerClient) : IContentStore
{
    private readonly SemaphoreSlim _containerExistsLock = new(1, 1);
    private readonly string _prefix = Guard.AgainstNull(azureContentStoreOptions).Prefix.Trim('/');
    private bool _containerExists = !azureContentStoreOptions.CreateContainerIfNotExists;

    public string Name { get; } = Guard.AgainstEmpty(name);

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureContainerExistsAsync(cancellationToken);

        try
        {
            var response = await GetBlobClient(key).DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            throw new FileNotFoundException($"Content not found for key '{key}' in store '{Name}'.", key, ex);
        }
    }

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);
        Guard.AgainstNull(content);

        await EnsureContainerExistsAsync(cancellationToken);

        await GetBlobClient(key).UploadAsync(content, overwrite: true, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureContainerExistsAsync(cancellationToken);

        return (await GetBlobClient(key).ExistsAsync(cancellationToken)).Value;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureContainerExistsAsync(cancellationToken);

        await GetBlobClient(key).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task EnsureContainerExistsAsync(CancellationToken cancellationToken)
    {
        if (_containerExists)
        {
            return;
        }

        await _containerExistsLock.WaitAsync(cancellationToken);

        try
        {
            if (_containerExists)
            {
                return;
            }

            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            _containerExists = true;
        }
        finally
        {
            _containerExistsLock.Release();
        }
    }

    private BlobClient GetBlobClient(string key) =>
        blobContainerClient.GetBlobClient(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}/{key}");
}
