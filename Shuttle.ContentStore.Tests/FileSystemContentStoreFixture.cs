using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.ContentStore.FileSystem;

namespace Shuttle.ContentStore.Tests;

[TestFixture]
public class FileSystemContentStoreFixture : ContentStoreFixture
{
    private static readonly string RootPath = Path.Combine(AppContext.BaseDirectory, "file-system-content-store-fixture");

    [SetUp]
    public void SetUp()
    {
        DeleteRootPath();
    }

    [TearDown]
    public void TearDown()
    {
        DeleteRootPath();
    }

    private static void DeleteRootPath()
    {
        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, recursive: true);
        }
    }

    protected override IContentStore CreateContentStore() => new FileSystemContentStore("fixture", new() { RootPath = RootPath });

    [Test]
    public async Task Should_store_content_in_a_path_relative_to_the_root_path()
    {
        await CreateContentStore().SaveAsync("folder/file.txt", Encoding.UTF8.GetBytes("content"));

        Assert.That(File.Exists(Path.Combine(RootPath, "folder", "file.txt")), Is.True);
    }

    [TestCase("../outside.txt")]
    [TestCase("folder/../../outside.txt")]
    public void Should_not_allow_keys_that_resolve_outside_of_the_root_path(string key)
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await CreateContentStore().ExistsAsync(key));
    }

    [Test]
    public async Task Should_be_able_to_register_multiple_stores_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Shuttle:ContentStore:FileSystem:Stores:documents:RootPath"] = Path.Combine(RootPath, "documents"),
                ["Shuttle:ContentStore:FileSystem:Stores:invoices:RootPath"] = Path.Combine(RootPath, "invoices")
            })
            .Build();

        var services = new ServiceCollection();

        services.AddContentStore()
            .AddFileSystem(configuration.GetSection(FileSystemContentStoreOptions.SectionName))
            .SetDefault("documents");

        await using var serviceProvider = services.BuildServiceProvider();

        var provider = serviceProvider.GetRequiredService<IContentStoreProvider>();

        await serviceProvider.GetRequiredService<IContentStore>().SaveAsync("file.txt", Encoding.UTF8.GetBytes("document"));
        await provider.Get("invoices").SaveAsync("file.txt", Encoding.UTF8.GetBytes("invoice"));

        Assert.That(await File.ReadAllTextAsync(Path.Combine(RootPath, "documents", "file.txt")), Is.EqualTo("document"));
        Assert.That(await File.ReadAllTextAsync(Path.Combine(RootPath, "invoices", "file.txt")), Is.EqualTo("invoice"));
    }
}
