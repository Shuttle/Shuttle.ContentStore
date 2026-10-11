using Azure.Core;

namespace Shuttle.ContentStore.Azure;

/// <summary>
/// Connection details for an Azure storage account, registered as named options where the name is the account
/// name referenced by <see cref="AzureContentStoreOptions.Account"/>.
/// </summary>
/// <remarks>
/// When <see cref="StorageAccount"/> or <see cref="ServiceUri"/> is set the endpoint is used with
/// <see cref="AccountKey"/> (shared key, requires <see cref="StorageAccount"/>), else <see cref="TokenCredential"/>,
/// else a shared <c>DefaultAzureCredential</c>.  Otherwise <see cref="ConnectionString"/> is used.
/// </remarks>
public class AzureStorageAccountOptions
{
    public string StorageAccount { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Overrides the default <c>https://{StorageAccount}.blob.core.windows.net</c> endpoint, e.g. for sovereign clouds or Azurite.
    /// </summary>
    public Uri? ServiceUri { get; set; }

    public TokenCredential? TokenCredential { get; set; }
}
