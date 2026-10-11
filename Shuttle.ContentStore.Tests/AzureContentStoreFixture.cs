using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.ContentStore.Azure;

namespace Shuttle.ContentStore.Tests;

[TestFixture]
public class AzureContentStoreFixture : ContentStoreFixture
{
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private const string ContainerName = "content-store-fixture";
    private const string OtherContainerName = "content-store-fixture-other";

    private readonly BlobServiceClient _blobServiceClient = new(ConnectionString);
    private BlobContainerClient Container => _blobServiceClient.GetBlobContainerClient(ContainerName);
    private BlobContainerClient OtherContainer => _blobServiceClient.GetBlobContainerClient(OtherContainerName);

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            await Container.DeleteIfExistsAsync();
            await OtherContainer.DeleteIfExistsAsync();
        }
        catch
        {
            // Ignore cleanup errors in teardown to prevent test suite crashes
        }
    }

    protected override IContentStore CreateContentStore() => CreateContentStore(string.Empty);

    private static IContentStore CreateContentStore(string prefix)
    {
        var services = new ServiceCollection();

        services.AddContentStore()
            .AddAzureStore("fixture", account => account.ConnectionString = ConnectionString, store =>
            {
                store.ContainerName = ContainerName;
                store.Prefix = prefix;
            });

        return services.BuildServiceProvider().GetRequiredService<IContentStore>();
    }

    [Test]
    public async Task Should_apply_the_prefix_to_the_blob_name()
    {
        const string key = "test-file-with-prefix.txt";

        var store = CreateContentStore("/fixture-prefix/");

        await store.SaveAsync(key, Encoding.UTF8.GetBytes("Hello with prefix!"));

        Assert.That((await Container.GetBlobClient($"fixture-prefix/{key}").ExistsAsync()).Value, Is.True);
    }

    [Test]
    public async Task Should_create_container_when_it_does_not_exist_on_first_access()
    {
        await Container.DeleteIfExistsAsync();

        Assert.That((await Container.ExistsAsync()).Value, Is.False);

        await CreateContentStore().SaveAsync("container-creation-test.txt", Encoding.UTF8.GetBytes("content"));

        Assert.That((await Container.ExistsAsync()).Value, Is.True);
    }

    [Test]
    public async Task Should_be_able_to_register_multiple_stores_sharing_an_account_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Shuttle:ContentStore:Azure:Accounts:local:ConnectionString"] = ConnectionString,
                ["Shuttle:ContentStore:Azure:Stores:documents:Account"] = "local",
                ["Shuttle:ContentStore:Azure:Stores:documents:ContainerName"] = ContainerName,
                ["Shuttle:ContentStore:Azure:Stores:invoices:Account"] = "local",
                ["Shuttle:ContentStore:Azure:Stores:invoices:ContainerName"] = OtherContainerName,
                ["Shuttle:ContentStore:Azure:Stores:invoices:Prefix"] = "invoices"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddContentStore().AddAzure(configuration.GetSection(AzureContentStoreOptions.SectionName));

        await using var serviceProvider = services.BuildServiceProvider();

        var provider = serviceProvider.GetRequiredService<IContentStoreProvider>();

        await provider.Get("documents").SaveAsync("file.txt", Encoding.UTF8.GetBytes("document"));
        await provider.Get("invoices").SaveAsync("file.txt", Encoding.UTF8.GetBytes("invoice"));

        Assert.That((await Container.GetBlobClient("file.txt").ExistsAsync()).Value, Is.True);
        Assert.That((await OtherContainer.GetBlobClient("invoices/file.txt").ExistsAsync()).Value, Is.True);
        Assert.That(await provider.Get("documents").GetTextAsync("file.txt"), Is.EqualTo("document"));
        Assert.That(await provider.Get("invoices").GetTextAsync("file.txt"), Is.EqualTo("invoice"));

        var clientProvider = serviceProvider.GetRequiredService<IAzureBlobServiceClientProvider>();

        Assert.That(clientProvider.Get("local"), Is.SameAs(clientProvider.Get("local")));
    }

    [Test]
    public void Should_fail_validation_when_a_store_references_an_unknown_account()
    {
        var services = new ServiceCollection();

        services.AddContentStore()
            .AddAzureStore("documents", store =>
            {
                store.Account = "unknown";
                store.ContainerName = ContainerName;
            });

        using var serviceProvider = services.BuildServiceProvider();

        var ex = Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<IOptionsMonitor<AzureContentStoreOptions>>().Get("documents"));

        Assert.That(ex!.Message, Does.Contain("unknown"));
    }
}
