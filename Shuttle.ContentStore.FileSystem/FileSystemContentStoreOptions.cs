using System.ComponentModel.DataAnnotations;

namespace Shuttle.ContentStore.FileSystem;

/// <summary>
/// Registered as named options where the name is the logical store name.
/// </summary>
public class FileSystemContentStoreOptions
{
    /// <summary>
    /// The configuration section containing the <c>Stores</c> passed to <c>AddFileSystem</c>.
    /// </summary>
    public const string SectionName = "Shuttle:ContentStore:FileSystem";

    /// <summary>
    /// The folder in which content is stored.  A relative path is resolved against <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    [Required]
    public string RootPath { get; set; } = string.Empty;
}
