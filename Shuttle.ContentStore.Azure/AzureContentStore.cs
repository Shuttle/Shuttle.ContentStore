using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using Azure.Storage;
using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.Azure;

public class AzureContentStore(IOptions<AzureContentStoreOptions> azureContentStoreOptions) : IContentStore
{
    private readonly BlobContainerClient _container = BuildContainerClient(azureContentStoreOptions.Value);

    public string Name { get; } = "azure";

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
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

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default) =>
        await _container.GetBlobClient(BlobKey(key)).UploadAsync(content, overwrite: true, cancellationToken);

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        (await _container.GetBlobClient(BlobKey(key)).ExistsAsync(cancellationToken)).Value;

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        await _container.GetBlobClient(BlobKey(key)).DeleteIfExistsAsync(cancellationToken: cancellationToken);

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