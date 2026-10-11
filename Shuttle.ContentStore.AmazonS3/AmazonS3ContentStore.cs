using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Shuttle.Contract;

namespace Shuttle.ContentStore.AmazonS3;

public class AmazonS3ContentStore(string name, AmazonS3ContentStoreOptions amazonS3ContentStoreOptions, IAmazonS3 amazonS3) : IContentStore
{
    private readonly SemaphoreSlim _bucketExistsLock = new(1, 1);
    private readonly string _prefix = Guard.AgainstNull(amazonS3ContentStoreOptions).Prefix.Trim('/');
    private bool _bucketExists = !amazonS3ContentStoreOptions.CreateBucketIfNotExists;

    public string Name { get; } = Guard.AgainstEmpty(name);

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureBucketExistsAsync(cancellationToken);

        try
        {
            var response = await amazonS3.GetObjectAsync(amazonS3ContentStoreOptions.BucketName, ObjectKey(key), cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"Content not found for key '{key}' in store '{Name}'.", key, ex);
        }
    }

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);
        Guard.AgainstNull(content);

        await EnsureBucketExistsAsync(cancellationToken);

        MemoryStream? buffer = null;

        try
        {
            // S3 requires a known content length, so non-seekable streams are buffered.
            if (!content.CanSeek)
            {
                buffer = new();

                await content.CopyToAsync(buffer, cancellationToken);

                buffer.Position = 0;
            }

            await amazonS3.PutObjectAsync(new()
            {
                BucketName = amazonS3ContentStoreOptions.BucketName,
                Key = ObjectKey(key),
                InputStream = buffer ?? content,
                AutoCloseStream = false
            }, cancellationToken);
        }
        finally
        {
            if (buffer != null)
            {
                await buffer.DisposeAsync();
            }
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureBucketExistsAsync(cancellationToken);

        try
        {
            await amazonS3.GetObjectMetadataAsync(amazonS3ContentStoreOptions.BucketName, ObjectKey(key), cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstEmpty(key);

        await EnsureBucketExistsAsync(cancellationToken);

        await amazonS3.DeleteObjectAsync(amazonS3ContentStoreOptions.BucketName, ObjectKey(key), cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (_bucketExists)
        {
            return;
        }

        await _bucketExistsLock.WaitAsync(cancellationToken);

        try
        {
            if (_bucketExists)
            {
                return;
            }

            try
            {
                await amazonS3.PutBucketAsync(new PutBucketRequest { BucketName = amazonS3ContentStoreOptions.BucketName }, cancellationToken);
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
            {
            }

            _bucketExists = true;
        }
        finally
        {
            _bucketExistsLock.Release();
        }
    }

    private string ObjectKey(string key) =>
        string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}/{key}";
}
