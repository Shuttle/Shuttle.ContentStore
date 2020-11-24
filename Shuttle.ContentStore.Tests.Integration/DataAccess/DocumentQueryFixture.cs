using System;
using System.Linq;
using Shuttle.ContentStore.DataAccess;
using Shuttle.ContentStore.DataAccess.Query;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests.Integration.DataAccess
{
    public class DocumentQueryFixture : DataAccessFixture
    {
        private readonly DocumentFixture _documentFixture = new DocumentFixture();

        [Test]
        public void Should_be_able_to_get_cleared_document_content()
        {
            var document = _documentFixture.GetDocument(Guid.NewGuid());
            var repository = new DocumentRepository(DatabaseGateway, new DocumentQueryFactory());
            var query = new DocumentQuery(QueryMapper, new DocumentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(query.FindContent(document.Id), Is.Null);

                repository.Save(document);

                DocumentContent content = null;

                Assert.That(() => content = query.FindContent(document.Id), Throws.Nothing);
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Status, Is.EqualTo("Registered"));
                Assert.That(content.Content, Is.Null);
                Assert.That(content.ContentType, Is.EqualTo(document.ContentType));

                document.Cleared();

                repository.Save(document);

                Assert.That(() => content = query.FindContent(document.Id), Throws.Nothing);
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Status, Is.EqualTo("Cleared"));
                Assert.That(content.Content, Is.Not.Null);
                Assert.That(content.ContentType, Is.EqualTo(document.ContentType));
            }
        }

        [Test]
        public void Should_be_able_to_get_sanitized_document_content()
        {
            var document = _documentFixture.GetDocument(Guid.NewGuid(),Guid.NewGuid());
            var repository = new DocumentRepository(DatabaseGateway, new DocumentQueryFactory());
            var query = new DocumentQuery(QueryMapper, new DocumentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(query.FindContent(document.Id), Is.Null);

                repository.Save(document);

                DocumentContent content = null;

                Assert.That(() => content = query.FindContent(document.Id), Throws.Nothing);
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Status, Is.EqualTo("Registered"));
                Assert.That(content.Content, Is.Null);
                Assert.That(content.ContentType, Is.EqualTo(document.ContentType));

                document.Suspicious();

                repository.Save(document);

                Assert.That(() => content = query.FindContent(document.ReferenceId), Throws.Nothing);
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Status, Is.EqualTo("Suspicious"));
                Assert.That(content.Content, Is.Null);
                Assert.That(content.ContentType, Is.EqualTo(document.ContentType));

                document.WithSanitizedContent(document.Content);

                repository.Save(document);

                Assert.That(() => content = query.FindContent(document.Id), Throws.Nothing);
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Status, Is.EqualTo("Suspicious"));
                Assert.That(content.Content, Is.Not.Null);
                Assert.That(content.ContentType, Is.EqualTo(document.ContentType));
            }
        }

        [Test]
        public void Should_be_able_to_retrieve_same_document_using_either_id_or_reference_id()
        {
            var referenceId = Guid.NewGuid();
            var documentA = _documentFixture.GetDocument(Guid.NewGuid(), referenceId);
            var documentB = _documentFixture.GetDocument(Guid.NewGuid(), referenceId);
            var repository = new DocumentRepository(DatabaseGateway, new DocumentQueryFactory());
            var query = new DocumentQuery(QueryMapper, new DocumentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                repository.Save(documentA);
                repository.Save(documentB);

                var result = query.Search(new DocumentStore.DataAccess.Query.Document.Specification().AddId(Guid.NewGuid())).ToList();

                Assert.That(result.Count, Is.Zero);
                
                result = query.Search(new DocumentStore.DataAccess.Query.Document.Specification().AddId(documentA.Id)).ToList();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(documentA.Id));
                Assert.That(result[0].ReferenceId, Is.EqualTo(referenceId));
                
                result = query.Search(new DocumentStore.DataAccess.Query.Document.Specification().AddId(documentB.Id)).ToList();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(documentB.Id));
                Assert.That(result[0].ReferenceId, Is.EqualTo(referenceId));
                
                result = query.Search(new DocumentStore.DataAccess.Query.Document.Specification().AddId(referenceId)).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                result = query.Search(new DocumentStore.DataAccess.Query.Document.Specification().AddId(referenceId).GetActiveOnly()).ToList();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(documentB.Id));
                Assert.That(result[0].ReferenceId, Is.EqualTo(referenceId));
            }
        }

        [Test]
        public void Should_be_able_to_search_for_documents()
        {
            var documentA = _documentFixture.GetDocument(Guid.NewGuid()).SetProperty("property-one", "value-one");
            var documentB = _documentFixture.GetDocument(Guid.NewGuid()).SetProperty("property-two", "value-two");
            var repository = new DocumentRepository(DatabaseGateway, new DocumentQueryFactory());
            var query = new DocumentQuery(QueryMapper, new DocumentQueryFactory());
            var specification = new DocumentStore.DataAccess.Query.Document.Specification()
                .AddId(documentA.Id)
                .AddId(documentB.Id);

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                var result = query.Search(specification).ToList();

                Assert.That(result.Any(), Is.False);

                repository.Save(documentA);

                result = query.Search(specification).ToList();

                Assert.That(result.Count, Is.EqualTo(1));

                repository.Save(documentB);

                result = query.Search(specification).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                foreach (var document in result)
                {
                    Assert.That(document.StatusEvents, Is.Empty);
                    Assert.That(document.Properties, Is.Empty);
                }

                result = query.Search(specification.IncludeStatusEvents()).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                foreach (var document in result)
                {
                    Assert.That(document.StatusEvents, Is.Not.Empty);
                    Assert.That(document.Properties, Is.Empty);
                }

                result = query.Search(specification.IncludeProperties()).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                foreach (var document in result)
                {
                    Assert.That(document.StatusEvents, Is.Not.Empty);
                    Assert.That(document.Properties, Is.Not.Empty);
                }
            }
        }
    }
}