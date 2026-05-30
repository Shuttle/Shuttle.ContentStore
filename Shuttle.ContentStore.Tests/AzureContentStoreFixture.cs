using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.ContentStore.Azure;

namespace Shuttle.ContentStore.Tests;

[TestFixture]
public class AzureContentStoreFixture
{
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private const string ContainerName = "content-store-fixture";
    private BlobContainerClient? _containerClient;

    [SetUp]
    public async Task SetUp()
    {
        _containerClient = new(ConnectionString, ContainerName);
        
        await _containerClient.CreateIfNotExistsAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_containerClient == null)
        {
            return;
        }

        try
        {
            await _containerClient.DeleteIfExistsAsync();
        }
        catch
        {
            // Ignore cleanup errors in teardown to prevent test suite crashes
        }
    }

    private static AzureContentStore CreateContentStore(string prefix = "")
    {
        var options = new AzureContentStoreOptions
        {
            ConnectionString = ConnectionString,
            ContainerName = ContainerName,
            Prefix = prefix
        };

        return new(Options.Create(options));
    }

    [Test]
    public async Task Should_be_able_to_put_exists_openread_and_delete_content_without_prefix()
    {
        const string key = "test-file.txt";
        const string contentString = "Hello, World! This is some test content.";

        var store = CreateContentStore();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(contentString));

        await store.PutAsync(key, stream);

        Assert.That(await store.ExistsAsync(key), Is.True);

        var blobClient = _containerClient!.GetBlobClient(key);

        Assert.That((await blobClient.ExistsAsync()).Value, Is.True);

        await using var readStream = await store.OpenReadAsync(key);
        using var reader = new StreamReader(readStream, Encoding.UTF8);
        var readContent = await reader.ReadToEndAsync();
        
        Assert.That(readContent, Is.EqualTo(contentString));

        await store.DeleteAsync(key);

        Assert.That(await store.ExistsAsync(key), Is.False);
        Assert.That((await blobClient.ExistsAsync()).Value, Is.False);
    }

    [Test]
    public async Task Should_be_able_to_put_exists_openread_and_delete_content_with_prefix()
    {
        const string prefix = "fixture-prefix";
        const string key = "test-file-with-prefix.txt";
        const string contentString = "Hello with prefix!";

        var store = CreateContentStore(prefix);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(contentString));

        await store.PutAsync(key, stream);

        Assert.That(await store.ExistsAsync(key), Is.True);

        const string expectedBlobKey = $"{prefix}/{key}";
        var blobClient = _containerClient!.GetBlobClient(expectedBlobKey);
        
        Assert.That((await blobClient.ExistsAsync()).Value, Is.True);

        await using var readStream = await store.OpenReadAsync(key);
        using var reader = new StreamReader(readStream, Encoding.UTF8);
        var readContent = await reader.ReadToEndAsync();
        
        Assert.That(readContent, Is.EqualTo(contentString));

        await store.DeleteAsync(key);

        Assert.That(await store.ExistsAsync(key), Is.False);
        Assert.That((await blobClient.ExistsAsync()).Value, Is.False);
    }

    [Test]
    public void Should_throw_FileNotFoundException_when_blob_does_not_exist()
    {
        var store = CreateContentStore();

        const string nonExistentKey = "does-not-exist.txt";

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () => await store.OpenReadAsync(nonExistentKey));

        Assert.That(ex!.Message, Does.Contain(nonExistentKey));
    }

    [Test]
    public async Task Should_overwrite_existing_blob_on_put()
    {
        var store = CreateContentStore();

        const string key = "overwrite-test.txt";
        
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("First Content"));
        
        await store.PutAsync(key, stream1);

        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("Second Content"));
        
        await store.PutAsync(key, stream2);

        await using var readStream = await store.OpenReadAsync(key);
        using var reader = new StreamReader(readStream, Encoding.UTF8);
        var readContent = await reader.ReadToEndAsync();

        Assert.That(readContent, Is.EqualTo("Second Content"));
    }
}
