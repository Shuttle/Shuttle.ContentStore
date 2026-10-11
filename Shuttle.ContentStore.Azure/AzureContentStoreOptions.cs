using System.ComponentModel.DataAnnotations;

namespace Shuttle.ContentStore.Azure;

/// <summary>
/// Registered as named options where the name is the logical store name.
/// </summary>
public class AzureContentStoreOptions
{
    /// <summary>
    /// The configuration section containing the <c>Accounts</c> and <c>Stores</c> passed to <c>AddAzure</c>.
    /// </summary>
    public const string SectionName = "Shuttle:ContentStore:Azure";

    /// <summary>
    /// The name of the <see cref="AzureStorageAccountOptions"/> to use.
    /// </summary>
    [Required]
    public string Account { get; set; } = string.Empty;

    [Required]
    public string ContainerName { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;
    public bool CreateContainerIfNotExists { get; set; } = true;
}
