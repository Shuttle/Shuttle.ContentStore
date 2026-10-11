using Amazon.Runtime;

namespace Shuttle.ContentStore.AmazonS3;

/// <summary>
/// Connection details for an S3 (or S3-compatible) endpoint, registered as named options where the name is the
/// account name referenced by <see cref="AmazonS3ContentStoreOptions.Account"/>.
/// </summary>
/// <remarks>
/// Credentials are resolved from <see cref="Credentials"/>, else <see cref="AccessKey"/>/<see cref="SecretKey"/>
/// (with optional <see cref="SessionToken"/>), else the named <see cref="Profile"/>, else the default AWS credential chain.
/// </remarks>
public class AmazonS3AccountOptions
{
    /// <summary>
    /// The AWS region system name, e.g. <c>eu-west-1</c>.  When <see cref="ServiceUrl"/> is set, this is used as the authentication region.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// A custom endpoint, e.g. for S3-compatible services such as MinIO or LocalStack.
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;

    public bool ForcePathStyle { get; set; }
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public AWSCredentials? Credentials { get; set; }
}
