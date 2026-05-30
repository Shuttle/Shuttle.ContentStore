using System.ComponentModel.DataAnnotations;

namespace Shuttle.ContentStore.Azure;

public class AzureContentStoreOptions
{
    public string AccountName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;

    [Required]
    public string ContainerName { get; set; } = string.Empty;
}
