using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.Azure;

public class AzureContentStoreOptionsValidator(IOptionsMonitor<AzureStorageAccountOptions> azureStorageAccountOptions) : IValidateOptions<AzureContentStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureContentStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Account))
        {
            return ValidateOptionsResult.Fail($"Azure content store '{name}' requires '{nameof(options.Account)}'.");
        }

        try
        {
            azureStorageAccountOptions.Get(options.Account);
        }
        catch (OptionsValidationException ex)
        {
            return ValidateOptionsResult.Fail($"Azure content store '{name}' references account '{options.Account}' which is not valid: {ex.Message}");
        }

        return ValidateOptionsResult.Success;
    }
}
