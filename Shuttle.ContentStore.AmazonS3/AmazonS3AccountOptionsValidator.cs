using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.AmazonS3;

public class AmazonS3AccountOptionsValidator : IValidateOptions<AmazonS3AccountOptions>
{
    public ValidateOptionsResult Validate(string? name, AmazonS3AccountOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AccessKey) != string.IsNullOrWhiteSpace(options.SecretKey))
        {
            return ValidateOptionsResult.Fail($"Amazon S3 account '{name}' must specify both '{nameof(options.AccessKey)}' and '{nameof(options.SecretKey)}', or neither.");
        }

        if (!string.IsNullOrWhiteSpace(options.SessionToken) && string.IsNullOrWhiteSpace(options.AccessKey))
        {
            return ValidateOptionsResult.Fail($"Amazon S3 account '{name}' specifies '{nameof(options.SessionToken)}' which requires '{nameof(options.AccessKey)}' and '{nameof(options.SecretKey)}'.");
        }

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl) && !Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail($"Amazon S3 account '{name}' has a '{nameof(options.ServiceUrl)}' value '{options.ServiceUrl}' that is not an absolute URI.");
        }

        return ValidateOptionsResult.Success;
    }
}
