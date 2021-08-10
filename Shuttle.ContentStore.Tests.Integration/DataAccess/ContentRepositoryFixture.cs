using System;
using Shuttle.ContentStore.DataAccess;
using NUnit.Framework;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.Tests.Integration.DataAccess
{
    public class ContentRepositoryFixture : DataAccessFixture
    {
        private readonly ContentFixture _contentFixture = new ContentFixture();

        [Test]
        public void Should_be_able_store_different_versions_of_the_same_reference_id()
        {
            var repository = new ContentRepository(DatabaseGateway, new ContentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                var referenceId = Guid.NewGuid();
                var query = RawQuery.Create($"select count(*) from dbo.Content where ReferenceId = '{referenceId}'");

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.Zero);

                repository.Save(_contentFixture.GetContent(Guid.NewGuid(), referenceId));

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.EqualTo(1));

                repository.Save(_contentFixture.GetContent(Guid.NewGuid(), referenceId));

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.EqualTo(2));

                repository.Save(_contentFixture.GetContent(Guid.NewGuid(), referenceId));

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.EqualTo(3));
            }
        }

        [Test]
        public void Should_be_able_to_persist_and_retrieve_simple_content()
        {
            var content = _contentFixture.GetContent(Guid.NewGuid());
            var repository = new ContentRepository(DatabaseGateway, new ContentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                content.SetProperty("property-xyz", "value-xyz");

                repository.Save(content);

                var persisted = repository.Get(content.Id);

                _contentFixture.AssertContent(persisted, content.Id);

                Assert.That(persisted.GetPropertyValue("property-xyz"), Is.EqualTo("value-xyz"));

                content.SetProperty("property-one", "value-one");

                repository.SaveProperties(content);

                persisted = repository.Get(content.Id);

                _contentFixture.AssertContent(persisted, content.Id);

                Assert.That(persisted.GetPropertyValue("property-one"), Is.EqualTo("value-one"));
                Assert.That(persisted.ContainsProperty("property-two"), Is.False);

                content.SetProperty("property-one", "something-else");
                content.SetProperty("property-two", "value-two");

                repository.SaveProperties(content);

                persisted = repository.Get(content.Id);

                _contentFixture.AssertContent(persisted, content.Id);

                Assert.That(persisted.GetPropertyValue("property-one"), Is.EqualTo("something-else"));
                Assert.That(persisted.GetPropertyValue("property-two"), Is.EqualTo("value-two"));

                content.RemoveProperty("property-two");

                repository.SaveProperties(content);

                persisted = repository.Get(content.Id);

                _contentFixture.AssertContent(persisted, content.Id);

                Assert.That(persisted.GetPropertyValue("property-one"), Is.EqualTo("something-else"));
                Assert.That(persisted.ContainsProperty("property-two"), Is.False);
            }
        }
    }
}