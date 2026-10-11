using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.AmazonS3;

public class AmazonS3ContentStoreOptionsValidator(IOptionsMonitor<AmazonS3AccountOptions> amazonS3AccountOptions) : IValidateOptions<AmazonS3ContentStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, AmazonS3ContentStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Account))
        {
            return ValidateOptionsResult.Fail($"Amazon S3 content store '{name}' requires '{nameof(options.Account)}'.");
        }

        try
        {
            amazonS3AccountOptions.Get(options.Account);
        }
        catch (OptionsValidationException ex)
        {
            return ValidateOptionsResult.Fail($"Amazon S3 content store '{name}' references account '{options.Account}' which is not valid: {ex.Message}");
        }

        return ValidateOptionsResult.Success;
    }
}
