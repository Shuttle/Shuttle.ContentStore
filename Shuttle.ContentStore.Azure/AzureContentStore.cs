using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using Azure.Storage;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore.Azure;

public class AzureContentStore(IOptions<AzureContentStoreOptions> azureContentStoreOptions) : IContentStore
{
    public const string StoreName = "azure";

    private readonly BlobContainerClient _container = BuildContainerClient(azureContentStoreOptions.Value);
    private readonly SemaphoreSlim _containerExistsLock = new(1, 1);
    private bool _containerExists;

    public string Name { get; } = StoreName;

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureContainerExistsAsync(cancellationToken);

        try
        {
            var response = await _container.GetBlobClient(BlobKey(key)).DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            throw new FileNotFoundException($"Content not found for key '{key}'.", key, ex);
        }
    }

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);
        Guard.AgainstNull(content);

        await EnsureContainerExistsAsync(cancellationToken);

        await _container.GetBlobClient(BlobKey(key)).UploadAsync(content, overwrite: true, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureContainerExistsAsync(cancellationToken);

        return (await _container.GetBlobClient(BlobKey(key)).ExistsAsync(cancellationToken)).Value;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureContainerExistsAsync(cancellationToken);

        await _container.GetBlobClient(BlobKey(key)).DeleteIfExistsAsync(cancellationToken: cancellationToken);
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

            await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            _containerExists = true;
        }
        finally
        {
            _containerExistsLock.Release();
        }
    }

    private string BlobKey(string key) =>
        string.IsNullOrEmpty(azureContentStoreOptions.Value.Prefix) ? key : $"{azureContentStoreOptions.Value.Prefix}/{key}";

    private static BlobContainerClient BuildContainerClient(AzureContentStoreOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.AccountName) && !string.IsNullOrWhiteSpace(options.AccountKey))
        {
            return new(new($"https://{options.AccountName}.blob.core.windows.net/{options.ContainerName}"), new StorageSharedKeyCredential(options.AccountName, options.AccountKey));
        }

        if (!string.IsNullOrWhiteSpace(options.AccountName))
        {
            return new(
                new($"https://{options.AccountName}.blob.core.windows.net/{options.ContainerName}"),
                new DefaultAzureCredential());
        }

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return new(options.ConnectionString, options.ContainerName);
        }

        throw new InvalidOperationException($"Configure either '{nameof(options.AccountName)}' (with optional '{nameof(options.AccountKey)}') or '{nameof(options.ConnectionString)}'.");
    }
}