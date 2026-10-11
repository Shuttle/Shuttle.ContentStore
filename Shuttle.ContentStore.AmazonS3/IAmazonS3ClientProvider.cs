using Amazon.S3;

namespace Shuttle.ContentStore.AmazonS3;

public interface IAmazonS3ClientProvider
{
    IAmazonS3 Get(string account);
}
