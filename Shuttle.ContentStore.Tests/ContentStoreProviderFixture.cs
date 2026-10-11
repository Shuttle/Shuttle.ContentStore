using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests;

[TestFixture]
public class ContentStoreProviderFixture
{
    private static ServiceProvider BuildServiceProvider(Action<ContentStoreBuilder> configure)
    {
        var services = new ServiceCollection();

        configure(services.AddContentStore());

        return services.BuildServiceProvider();
    }

    private static ContentStoreBuilder AddFake(ContentStoreBuilder builder, string name) =>
        builder.AddContentStore(name, (_, storeName) => new FakeContentStore(storeName));

    [Test]
    public void Should_use_the_only_registered_store_as_the_default()
    {
        using var serviceProvider = BuildServiceProvider(builder => AddFake(builder, "documents"));

        var store = serviceProvider.GetRequiredService<IContentStore>();

        Assert.That(store.Name, Is.EqualTo("documents"));
        Assert.That(store, Is.SameAs(serviceProvider.GetRequiredKeyedService<IContentStore>("documents")));
    }

    [Test]
    public void Should_throw_when_resolving_the_default_store_with_multiple_stores_and_no_default()
    {
        using var serviceProvider = BuildServiceProvider(builder => AddFake(AddFake(builder, "documents"), "invoices"));

        var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IContentStore>());

        Assert.That(ex!.Message, Does.Contain("'documents'").And.Contain("'invoices'"));
    }

    [Test]
    public void Should_resolve_the_specified_default_store()
    {
        using var serviceProvider = BuildServiceProvider(builder => AddFake(AddFake(builder, "documents"), "invoices").SetDefault("invoices"));

        Assert.That(serviceProvider.GetRequiredService<IContentStore>().Name, Is.EqualTo("invoices"));
    }

    [Test]
    public void Should_be_able_to_resolve_stores_by_name()
    {
        using var serviceProvider = BuildServiceProvider(builder => AddFake(AddFake(builder, "documents"), "invoices"));

        var provider = serviceProvider.GetRequiredService<IContentStoreProvider>();

        Assert.That(provider.Names, Is.EquivalentTo(new[] { "documents", "invoices" }));
        Assert.That(provider.Contains("invoices"), Is.True);
        Assert.That(provider.Contains("archive"), Is.False);
        Assert.That(provider.Get("invoices"), Is.SameAs(serviceProvider.GetRequiredKeyedService<IContentStore>("invoices")));
        Assert.Throws<InvalidOperationException>(() => provider.Get("archive"));
    }

    [Test]
    public void Should_not_allow_duplicate_store_names()
    {
        Assert.Throws<InvalidOperationException>(() => BuildServiceProvider(builder => AddFake(AddFake(builder, "documents"), "documents")));
    }

    [Test]
    public void Should_fail_validation_when_the_default_store_has_not_been_registered()
    {
        using var serviceProvider = BuildServiceProvider(builder => AddFake(builder, "documents").SetDefault("archive"));

        Assert.Throws<OptionsValidationException>(() => _ = serviceProvider.GetRequiredService<IOptions<ContentStoreOptions>>().Value);
    }

    private sealed class FakeContentStore(string name) : IContentStore
    {
        public string Name { get; } = name;
        public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
