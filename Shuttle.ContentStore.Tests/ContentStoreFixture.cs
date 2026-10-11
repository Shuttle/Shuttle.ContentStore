using System.Text;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests;

/// <summary>
/// Behaviour that every <see cref="IContentStore"/> implementation must exhibit.
/// </summary>
public abstract class ContentStoreFixture
{
    protected abstract IContentStore CreateContentStore();

    [Test]
    public async Task Should_be_able_to_put_exists_openread_and_delete_content()
    {
        const string key = "folder/test-file.txt";
        const string content = "Hello, World! This is some test content.";

        var store = CreateContentStore();

        await store.PutAsync(key, new MemoryStream(Encoding.UTF8.GetBytes(content)));

        Assert.That(await store.ExistsAsync(key), Is.True);
        Assert.That(await store.GetTextAsync(key), Is.EqualTo(content));

        await store.DeleteAsync(key);

        Assert.That(await store.ExistsAsync(key), Is.False);
    }

    [Test]
    public void Should_throw_FileNotFoundException_when_content_does_not_exist()
    {
        const string key = "does-not-exist.txt";

        var store = CreateContentStore();

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () => await store.OpenReadAsync(key));

        Assert.That(ex!.Message, Does.Contain(key));
    }

    [Test]
    public async Task Should_not_throw_when_deleting_content_that_does_not_exist()
    {
        await CreateContentStore().DeleteAsync("does-not-exist.txt");
    }

    [Test]
    public async Task Should_overwrite_existing_content_on_put()
    {
        const string key = "overwrite-test.txt";

        var store = CreateContentStore();

        await store.SaveAsync(key, Encoding.UTF8.GetBytes("First Content"));
        await store.SaveAsync(key, Encoding.UTF8.GetBytes("Second Content"));

        Assert.That(await store.GetTextAsync(key), Is.EqualTo("Second Content"));
    }

    [Test]
    public async Task Should_be_able_to_put_a_non_seekable_stream()
    {
        const string key = "non-seekable.txt";
        const string content = "Non-seekable content.";

        var store = CreateContentStore();

        await store.PutAsync(key, new NonSeekableStream(new MemoryStream(Encoding.UTF8.GetBytes(content))));

        Assert.That(await store.GetTextAsync(key), Is.EqualTo(content));
    }

    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
