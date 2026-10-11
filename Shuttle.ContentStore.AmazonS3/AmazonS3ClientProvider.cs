using System.Collections.Concurrent;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore.AmazonS3;

/// <summary>
/// Caches a single <see cref="IAmazonS3"/> client per account so that all stores using the same account share
/// connections and credentials.
/// </summary>
public class AmazonS3ClientProvider(IOptionsMonitor<AmazonS3AccountOptions> amazonS3AccountOptions) : IAmazonS3ClientProvider, IDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<IAmazonS3>> _clients = new(StringComparer.Ordinal);

    public void Dispose()
    {
        foreach (var client in _clients.Values.Where(client => client.IsValueCreated))
        {
            client.Value.Dispose();
        }

        _clients.Clear();
    }

    public IAmazonS3 Get(string account)
    {
        Guard.AgainstEmpty(account);

        return _clients.GetOrAdd(account, name => new(() => Create(name, amazonS3AccountOptions.Get(name)))).Value;
    }

    private static IAmazonS3 Create(string account, AmazonS3AccountOptions options)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = options.ForcePathStyle
        };

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            config.ServiceURL = options.ServiceUrl;

            if (!string.IsNullOrWhiteSpace(options.Region))
            {
                config.AuthenticationRegion = options.Region;
            }
        }
        else if (!string.IsNullOrWhiteSpace(options.Region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }

        var credentials = GetCredentials(account, options);

        return credentials == null
            ? new AmazonS3Client(config)
            : new AmazonS3Client(credentials, config);
    }

    private static AWSCredentials? GetCredentials(string account, AmazonS3AccountOptions options)
    {
        if (options.Credentials != null)
        {
            return options.Credentials;
        }

        if (!string.IsNullOrWhiteSpace(options.AccessKey))
        {
            return string.IsNullOrWhiteSpace(options.SessionToken)
                ? new BasicAWSCredentials(options.AccessKey, options.SecretKey)
                : new SessionAWSCredentials(options.AccessKey, options.SecretKey, options.SessionToken);
        }

        if (!string.IsNullOrWhiteSpace(options.Profile))
        {
            return new CredentialProfileStoreChain().TryGetAWSCredentials(options.Profile, out var profileCredentials)
                ? profileCredentials
                : throw new InvalidOperationException($"Amazon S3 account '{account}' references profile '{options.Profile}' which could not be found.");
        }

        return null;
    }
}
