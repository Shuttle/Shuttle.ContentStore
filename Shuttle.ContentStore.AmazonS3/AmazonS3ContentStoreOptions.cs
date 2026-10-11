using System.ComponentModel.DataAnnotations;

namespace Shuttle.ContentStore.AmazonS3;

/// <summary>
/// Registered as named options where the name is the logical store name.
/// </summary>
public class AmazonS3ContentStoreOptions
{
    /// <summary>
    /// The configuration section containing the <c>Accounts</c> and <c>Stores</c> passed to <c>AddAmazonS3</c>.
    /// </summary>
    public const string SectionName = "Shuttle:ContentStore:AmazonS3";

    /// <summary>
    /// The name of the <see cref="AmazonS3AccountOptions"/> to use.
    /// </summary>
    [Required]
    public string Account { get; set; } = string.Empty;

    [Required]
    public string BucketName { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Bucket names are globally unique and creating them requires additional permissions, so this is off by default.
    /// </summary>
    public bool CreateBucketIfNotExists { get; set; }
}
