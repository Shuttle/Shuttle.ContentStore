using System.ComponentModel.DataAnnotations;

namespace Shuttle.ContentStore.Azure;

/// <summary>
/// When resolving credentials, <see cref="AccountName"/>+<see cref="AccountKey"/> take precedence over
/// <see cref="AccountName"/> alone (which uses <c>DefaultAzureCredential</c>), which in turn takes precedence
/// over <see cref="ConnectionString"/>. Only one of these approaches should be configured at a time.
/// </summary>
public class AzureContentStoreOptions
{
    public const string SectionName = "Shuttle:ContentStore:Azure";

    public string AccountName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;

    [Required]
    public string ContainerName { get; set; } = string.Empty;
}
