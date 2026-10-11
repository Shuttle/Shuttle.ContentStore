using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.Azure;

public class AzureStorageAccountOptionsValidator : IValidateOptions<AzureStorageAccountOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureStorageAccountOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.StorageAccount) &&
            options.ServiceUri == null &&
            string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail($"Azure storage account '{name}' requires '{nameof(options.StorageAccount)}' or '{nameof(options.ServiceUri)}' (with optional '{nameof(options.AccountKey)}' or '{nameof(options.TokenCredential)}'), or '{nameof(options.ConnectionString)}'.");
        }

        if (!string.IsNullOrWhiteSpace(options.AccountKey) && string.IsNullOrWhiteSpace(options.StorageAccount))
        {
            return ValidateOptionsResult.Fail($"Azure storage account '{name}' specifies '{nameof(options.AccountKey)}' which requires '{nameof(options.StorageAccount)}'.");
        }

        return ValidateOptionsResult.Success;
    }
}
