using System.Collections.Concurrent;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore.Azure;

/// <summary>
/// Caches a single <see cref="BlobServiceClient"/> per account so that all stores using the same account share
/// connections and credentials.
/// </summary>
public class AzureBlobServiceClientProvider(IOptionsMonitor<AzureStorageAccountOptions> azureStorageAccountOptions) : IAzureBlobServiceClientProvider
{
    private readonly ConcurrentDictionary<string, Lazy<BlobServiceClient>> _clients = new(StringComparer.Ordinal);
    private readonly Lazy<TokenCredential> _defaultAzureCredential = new(() => new DefaultAzureCredential());

    public BlobServiceClient Get(string account)
    {
        Guard.AgainstEmpty(account);

        return _clients.GetOrAdd(account, name => new(() => Create(azureStorageAccountOptions.Get(name)))).Value;
    }

    private BlobServiceClient Create(AzureStorageAccountOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.StorageAccount) || options.ServiceUri != null)
        {
            var serviceUri = options.ServiceUri ?? new Uri($"https://{options.StorageAccount}.blob.core.windows.net");

            return !string.IsNullOrWhiteSpace(options.AccountKey)
                ? new(serviceUri, new StorageSharedKeyCredential(options.StorageAccount, options.AccountKey))
                : new(serviceUri, options.TokenCredential ?? _defaultAzureCredential.Value);
        }

        return new(options.ConnectionString);
    }
}
