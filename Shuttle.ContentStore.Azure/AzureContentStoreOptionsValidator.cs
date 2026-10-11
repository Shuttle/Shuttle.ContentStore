using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.Azure;

public class AzureContentStoreOptionsValidator : IValidateOptions<AzureContentStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureContentStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AccountKey) &&
            string.IsNullOrWhiteSpace(options.StorageAccount) &&
            string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail($"Options '{nameof(options.StorageAccount)}' (with optional '{nameof(options.AccountKey)}') or '{nameof(options.ConnectionString)}' must be specified.");
        }

        return ValidateOptionsResult.Success;
    }
}