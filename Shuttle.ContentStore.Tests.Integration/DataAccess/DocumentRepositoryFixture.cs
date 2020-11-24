using System;
using Shuttle.ContentStore.DataAccess;
using NUnit.Framework;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.Tests.Integration.DataAccess
{
    public class DocumentRepositoryFixture : DataAccessFixture
    {
        private readonly DocumentFixture _documentFixture = new DocumentFixture();

        [Test]
        public void Should_be_able_store_different_versions_of_the_same_reference_id()
        {
            var repository = new DocumentRepository(DatabaseGateway, new DocumentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                var referenceId = Guid.NewGuid();
                var query = RawQuery.Create($"select count(*) from dbo.Document where ReferenceId = '{referenceId}'");

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.Zero);

                repository.Save(_documentFixture.GetDocument(Guid.NewGuid(), referenceId));

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.EqualTo(1));

                repository.Save(_documentFixture.GetDocument(Guid.NewGuid(), referenceId));

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.EqualTo(2));

                repository.Save(_documentFixture.GetDocument(Guid.NewGuid(), referenceId));

                Assert.That(DatabaseGateway.GetScalarUsing<int>(query), Is.EqualTo(3));
            }
        }

        [Test]
        public void Should_be_able_to_persist_and_retrieve_simple_document()
        {
            var document = _documentFixture.GetDocument(Guid.NewGuid());
            var repository = new DocumentRepository(DatabaseGateway, new DocumentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                document.SetProperty("property-xyz", "value-xyz");

                repository.Save(document);

                var persisted = repository.Get(document.Id);

                _documentFixture.AssertDocument(persisted, document.Id);

                Assert.That(persisted.GetPropertyValue("property-xyz"), Is.EqualTo("value-xyz"));

                document.SetProperty("property-one", "value-one");

                repository.SaveProperties(document);

                persisted = repository.Get(document.Id);

                _documentFixture.AssertDocument(persisted, document.Id);

                Assert.That(persisted.GetPropertyValue("property-one"), Is.EqualTo("value-one"));
                Assert.That(persisted.ContainsProperty("property-two"), Is.False);

                document.SetProperty("property-one", "something-else");
                document.SetProperty("property-two", "value-two");

                repository.SaveProperties(document);

                persisted = repository.Get(document.Id);

                _documentFixture.AssertDocument(persisted, document.Id);

                Assert.That(persisted.GetPropertyValue("property-one"), Is.EqualTo("something-else"));
                Assert.That(persisted.GetPropertyValue("property-two"), Is.EqualTo("value-two"));

                document.RemoveProperty("property-two");

                repository.SaveProperties(document);

                persisted = repository.Get(document.Id);

                _documentFixture.AssertDocument(persisted, document.Id);

                Assert.That(persisted.GetPropertyValue("property-one"), Is.EqualTo("something-else"));
                Assert.That(persisted.ContainsProperty("property-two"), Is.False);
            }
        }
    }
}