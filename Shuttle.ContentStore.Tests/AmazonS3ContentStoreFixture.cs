using System.Net.Sockets;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.ContentStore.AmazonS3;

namespace Shuttle.ContentStore.Tests;

/// <summary>
/// Runs against LocalStack (see docker-compose.yml); the fixture is ignored when LocalStack is not available.
/// </summary>
[TestFixture]
public class AmazonS3ContentStoreFixture : ContentStoreFixture
{
    private const string ServiceUrl = "http://localhost:4566";
    private const string AccessKey = "test";
    private const string SecretKey = "test";
    private const string BucketName = "content-store-fixture";
    private const string OtherBucketName = "content-store-fixture-other";

    private ServiceProvider? _serviceProvider;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            using var tcpClient = new TcpClient();

            await tcpClient.ConnectAsync("localhost", 4566).WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch
        {
            Assert.Ignore($"LocalStack is not available at '{ServiceUrl}'.");
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
            _serviceProvider = null;
        }

        using var client = CreateClient();

        foreach (var bucketName in new[] { BucketName, OtherBucketName })
        {
            try
            {
                var objects = await client.ListObjectsV2Async(new() { BucketName = bucketName });

                foreach (var s3Object in objects.S3Objects ?? [])
                {
                    await client.DeleteObjectAsync(bucketName, s3Object.Key);
                }

                await client.DeleteBucketAsync(bucketName);
            }
            catch
            {
                // Ignore cleanup errors in teardown to prevent test suite crashes
            }
        }
    }

    private static IAmazonS3 CreateClient() =>
        new AmazonS3Client(AccessKey, SecretKey, new AmazonS3Config { ServiceURL = ServiceUrl, ForcePathStyle = true, AuthenticationRegion = "us-east-1" });

    private static void ConfigureAccount(AmazonS3AccountOptions options)
    {
        options.ServiceUrl = ServiceUrl;
        options.Region = "us-east-1";
        options.ForcePathStyle = true;
        options.AccessKey = AccessKey;
        options.SecretKey = SecretKey;
    }

    protected override IContentStore CreateContentStore() => CreateContentStore(string.Empty);

    private IContentStore CreateContentStore(string prefix)
    {
        var services = new ServiceCollection();

        services.AddContentStore()
            .AddAmazonS3Store("fixture", ConfigureAccount, store =>
            {
                store.BucketName = BucketName;
                store.Prefix = prefix;
                store.CreateBucketIfNotExists = true;
            });

        _serviceProvider = services.BuildServiceProvider();

        return _serviceProvider.GetRequiredService<IContentStore>();
    }

    [Test]
    public async Task Should_apply_the_prefix_to_the_object_key()
    {
        const string key = "test-file-with-prefix.txt";

        await CreateContentStore("fixture-prefix/").SaveAsync(key, Encoding.UTF8.GetBytes("Hello with prefix!"));

        using var client = CreateClient();

        Assert.That((await client.GetObjectMetadataAsync(BucketName, $"fixture-prefix/{key}")).HttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task Should_be_able_to_register_multiple_stores_sharing_an_account()
    {
        var services = new ServiceCollection();

        services.AddContentStore()
            .AddAmazonS3Account("local", ConfigureAccount)
            .AddAmazonS3Store("documents", store =>
            {
                store.Account = "local";
                store.BucketName = BucketName;
                store.CreateBucketIfNotExists = true;
            })
            .AddAmazonS3Store("invoices", store =>
            {
                store.Account = "local";
                store.BucketName = OtherBucketName;
                store.CreateBucketIfNotExists = true;
            })
            .SetDefault("documents");

        _serviceProvider = services.BuildServiceProvider();

        var provider = _serviceProvider.GetRequiredService<IContentStoreProvider>();

        await _serviceProvider.GetRequiredService<IContentStore>().SaveAsync("file.txt", Encoding.UTF8.GetBytes("document"));
        await provider.Get("invoices").SaveAsync("file.txt", Encoding.UTF8.GetBytes("invoice"));

        Assert.That(await provider.Get("documents").GetTextAsync("file.txt"), Is.EqualTo("document"));
        Assert.That(await provider.Get("invoices").GetTextAsync("file.txt"), Is.EqualTo("invoice"));

        using var client = CreateClient();

        await client.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = OtherBucketName, Key = "file.txt" });
    }
}
