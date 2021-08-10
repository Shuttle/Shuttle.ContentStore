using System;
using System.Linq;
using NUnit.Framework;
using Shuttle.ContentStore.DataAccess;
using Shuttle.ContentStore.DataAccess.Query;

namespace Shuttle.ContentStore.Tests.Integration.DataAccess
{
    public class ContentQueryFixture : DataAccessFixture
    {
        private readonly ContentFixture _contentFixture = new();

        [Test]
        public void Should_be_able_to_get_passed_content_bytes()
        {
            var content = _contentFixture.GetContent(Guid.NewGuid());
            var repository = new ContentRepository(DatabaseGateway, new ContentQueryFactory());
            var query = new ContentQuery(QueryMapper, new ContentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(query.FindRawContent(content.Id), Is.Null);

                repository.Save(content);

                RawContent rawContent = null;

                Assert.That(() => rawContent = query.FindRawContent(content.Id), Throws.Nothing);
                Assert.That(rawContent, Is.Not.Null);
                Assert.That(rawContent.Status, Is.EqualTo("Registered"));
                Assert.That(rawContent.Bytes, Is.Null);
                Assert.That(rawContent.ContentType, Is.EqualTo(content.ContentType));

                content.Passed();

                repository.Save(content);

                Assert.That(() => rawContent = query.FindRawContent(content.Id), Throws.Nothing);
                Assert.That(rawContent, Is.Not.Null);
                Assert.That(rawContent.Status, Is.EqualTo("Passed"));
                Assert.That(rawContent.Bytes, Is.Not.Null);
                Assert.That(rawContent.ContentType, Is.EqualTo(content.ContentType));
            }
        }

        [Test]
        public void Should_be_able_to_get_sanitized_content_bytes()
        
        {
            var content = _contentFixture.GetContent(Guid.NewGuid(), Guid.NewGuid());
            var repository = new ContentRepository(DatabaseGateway, new ContentQueryFactory());
            var query = new ContentQuery(QueryMapper, new ContentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                Assert.That(query.FindRawContent(content.Id), Is.Null);

                repository.Save(content);

                RawContent rawContent = null;

                Assert.That(() => rawContent = query.FindRawContent(content.Id), Throws.Nothing);
                Assert.That(rawContent, Is.Not.Null);
                Assert.That(rawContent.Status, Is.EqualTo("Registered"));
                Assert.That(rawContent.Bytes, Is.Null);
                Assert.That(rawContent.ContentType, Is.EqualTo(content.ContentType));

                content.Suspicious();

                repository.Save(content);

                Assert.That(() => rawContent = query.FindRawContent(content.ReferenceId), Throws.Nothing);
                Assert.That(rawContent, Is.Not.Null);
                Assert.That(rawContent.Status, Is.EqualTo("Suspicious"));
                Assert.That(rawContent.Bytes, Is.Null);
                Assert.That(rawContent.ContentType, Is.EqualTo(content.ContentType));

                content.WithSanitizedContent(content.Bytes);

                repository.Save(content);

                Assert.That(() => rawContent = query.FindRawContent(content.Id), Throws.Nothing);
                Assert.That(rawContent, Is.Not.Null);
                Assert.That(rawContent.Status, Is.EqualTo("Suspicious"));
                Assert.That(rawContent.Bytes, Is.Not.Null);
                Assert.That(rawContent.ContentType, Is.EqualTo(content.ContentType));
            }
        }

        [Test]
        public void Should_be_able_to_retrieve_same_content_using_either_id_or_reference_id()
        {
            var referenceId = Guid.NewGuid();
            var contentA = _contentFixture.GetContent(Guid.NewGuid(), referenceId);
            var contentB = _contentFixture.GetContent(Guid.NewGuid(), referenceId);
            var repository = new ContentRepository(DatabaseGateway, new ContentQueryFactory());
            var query = new ContentQuery(QueryMapper, new ContentQueryFactory());

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                repository.Save(contentA);
                repository.Save(contentB);

                var result = query
                    .Search(new ContentStore.DataAccess.Query.Content.Specification().AddId(Guid.NewGuid())).ToList();

                Assert.That(result.Count, Is.Zero);

                result = query.Search(new ContentStore.DataAccess.Query.Content.Specification().AddId(contentA.Id))
                    .ToList();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(contentA.Id));
                Assert.That(result[0].ReferenceId, Is.EqualTo(referenceId));

                result = query.Search(new ContentStore.DataAccess.Query.Content.Specification().AddId(contentB.Id))
                    .ToList();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(contentB.Id));
                Assert.That(result[0].ReferenceId, Is.EqualTo(referenceId));

                result = query.Search(new ContentStore.DataAccess.Query.Content.Specification().AddId(referenceId))
                    .ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                result = query.Search(new ContentStore.DataAccess.Query.Content.Specification().AddId(referenceId)
                    .GetActiveOnly()).ToList();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(contentB.Id));
                Assert.That(result[0].ReferenceId, Is.EqualTo(referenceId));
            }
        }

        [Test]
        public void Should_be_able_to_search_for_contents()
        {
            var contentA = _contentFixture.GetContent(Guid.NewGuid()).SetProperty("property-one", "value-one");
            var contentB = _contentFixture.GetContent(Guid.NewGuid()).SetProperty("property-two", "value-two");
            var repository = new ContentRepository(DatabaseGateway, new ContentQueryFactory());
            var query = new ContentQuery(QueryMapper, new ContentQueryFactory());
            var specification = new ContentStore.DataAccess.Query.Content.Specification()
                .AddId(contentA.Id)
                .AddId(contentB.Id);

            using (TransactionScopeFactory.Create())
            using (DatabaseContextFactory.Create())
            {
                var result = query.Search(specification).ToList();

                Assert.That(result.Any(), Is.False);

                repository.Save(contentA);

                result = query.Search(specification).ToList();

                Assert.That(result.Count, Is.EqualTo(1));

                repository.Save(contentB);

                result = query.Search(specification).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                foreach (var content in result)
                {
                    Assert.That(content.StatusEvents, Is.Empty);
                    Assert.That(content.Properties, Is.Empty);
                }

                result = query.Search(specification.IncludeStatusEvents()).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                foreach (var content in result)
                {
                    Assert.That(content.StatusEvents, Is.Not.Empty);
                    Assert.That(content.Properties, Is.Empty);
                }

                result = query.Search(specification.IncludeProperties()).ToList();

                Assert.That(result.Count, Is.EqualTo(2));

                foreach (var content in result)
                {
                    Assert.That(content.StatusEvents, Is.Not.Empty);
                    Assert.That(content.Properties, Is.Not.Empty);
                }
            }
        }
    }
}