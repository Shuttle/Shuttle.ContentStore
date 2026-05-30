using System.Text;

namespace Shuttle.ContentStore;

public static class ContentStoreExtensions
{
    extension(IContentStore store)
    {
        public async Task SaveAsync(string key, byte[] bytes, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(bytes);

            await using var stream = new MemoryStream(bytes, writable: false);

            await store.PutAsync(key, stream, cancellationToken);
        }

        public async Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(store);

            await using var stream = await store.OpenReadAsync(key, cancellationToken);

            if (stream is MemoryStream ms)
            {
                return ms.ToArray();
            }

            await using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken);
            return buffer.ToArray();
        }

        public async Task<string> GetTextAsync(string key, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(store);

            encoding ??= Encoding.UTF8;

            await using var stream = await store.OpenReadAsync(key, cancellationToken);
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);

            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}