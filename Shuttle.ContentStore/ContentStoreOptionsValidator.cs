using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore;

public class ContentStoreOptionsValidator : IValidateOptions<ContentStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, ContentStoreOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.DefaultStore) && !options.StoreNames.Contains(options.DefaultStore, StringComparer.Ordinal))
        {
            return ValidateOptionsResult.Fail($"The default content store '{options.DefaultStore}' has not been registered.");
        }

        return ValidateOptionsResult.Success;
    }
}
