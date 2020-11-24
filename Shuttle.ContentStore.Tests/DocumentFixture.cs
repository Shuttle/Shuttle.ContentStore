using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests
{
    [TestFixture]
    public class DocumentFixture
    {
        private readonly byte[] _content = {0, 1, 2, 3};
        private readonly Dictionary<Guid, DateTime> _effectiveFromDates =new Dictionary<Guid, DateTime>();
        private const string FileName = "file-name";
        private const string ContentType = "content-type";
        private const string SystemName = "system-name";
        private const string Username = "username";
        private const string PropertyOneName = "property-one";
        private const string PropertyTwoName = "property-two";

        public Document GetDocument(Guid id)
        {
            return GetDocument(id, id);
        }

        public Document GetDocument(Guid id, Guid referenceId)
        {
            _effectiveFromDates.Add(id, DateTime.Now);

            return new Document(id, referenceId, FileName, ContentType, _content, SystemName, Username, _effectiveFromDates[id]);
        }

        public void AssertDocument(Document document, Guid id)
        {
            Assert.That(document.Id, Is.EqualTo(id));
            Assert.That(document.ReferenceId, Is.EqualTo(id));
            Assert.That(document.FileName, Is.EqualTo(FileName));
            Assert.That(document.ContentType, Is.EqualTo(ContentType));
            Assert.That(document.Content, Is.Not.Null);
            Assert.That(document.Content, Is.EquivalentTo(_content));
            Assert.That(document.SystemName, Is.EqualTo(SystemName));
            Assert.That(document.Username, Is.EqualTo(Username));
            Assert.That(document.Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(document.StatusDateRegistered, Is.EqualTo(_effectiveFromDates[id]));
            Assert.That(document.EffectiveFromDate, Is.EqualTo(_effectiveFromDates[id]));
            Assert.That(document.EffectiveToDate, Is.EqualTo(DateTime.MaxValue));

            var statusEvents = document.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(1));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[id]));
        }

        public void AssertDocumentCleared(Document document)
        {
            var statusEvents = document.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(3));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[document.Id]));
            Assert.That(statusEvents[1].Status, Is.EqualTo(ServiceStatus.Processing));
            Assert.That(statusEvents[2].Status, Is.EqualTo(ServiceStatus.Cleared));

            Assert.That(document.HasSanitizedContent, Is.False);
            Assert.That(() => document.SanitizedContent, Throws.InvalidOperationException);
        }

        public void AssertDocumentSuspiciousButSanitized(Document document)
        {
            var statusEvents = document.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(3));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[document.Id]));
            Assert.That(statusEvents[1].Status, Is.EqualTo(ServiceStatus.Processing));
            Assert.That(statusEvents[2].Status, Is.EqualTo(ServiceStatus.Suspicious));

            Assert.That(document.HasSanitizedContent, Is.True);
            Assert.That(document.SanitizedContent, Is.EquivalentTo(_content));
        }

        public void AssertDocumentSuspicious(Document document)
        {
            var statusEvents = document.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(3));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[document.Id]));
            Assert.That(statusEvents[1].Status, Is.EqualTo(ServiceStatus.Processing));
            Assert.That(statusEvents[2].Status, Is.EqualTo(ServiceStatus.Suspicious));

            Assert.That(document.HasSanitizedContent, Is.False);
            Assert.That(() => document.SanitizedContent, Throws.InvalidOperationException);
        }

        [Test]
        public void Should_be_able_to_change_status_information_on_status_event()
        {
            var document = GetDocument(Guid.NewGuid());

            Assert.That(document.StatusDateRegistered, Is.EqualTo(_effectiveFromDates[document.Id]));

            var dateRegistered = DateTime.Now.AddSeconds(-5);

            document.OnStatusEvent(ServiceStatus.Cleared, dateRegistered);

            Assert.That(document.StatusDateRegistered, Is.Not.EqualTo(_effectiveFromDates[document.Id]));
            Assert.That(document.StatusDateRegistered, Is.EqualTo(dateRegistered));
        }

        [Test]
        public void Should_be_able_to_instantiate_valid_document()
        {
            var id = Guid.NewGuid();
            var document = GetDocument(id);

            AssertDocument(document, id);
        }

        [Test]
        public void Should_be_able_to_manage_a_property()
        {
            var document = GetDocument(Guid.NewGuid());

            Assert.That(document.GetProperties().Count, Is.Zero);
            Assert.That(() => document.GetPropertyValue(PropertyOneName), Throws.InvalidOperationException);
            Assert.That(() => document.GetPropertyValue(PropertyTwoName), Throws.InvalidOperationException);

            document.SetProperty(PropertyOneName, "value-one");

            Assert.That(document.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(() => document.GetPropertyValue(PropertyTwoName), Throws.InvalidOperationException);

            document.SetProperty(PropertyTwoName, "value-two");

            Assert.That(document.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(document.GetPropertyValue(PropertyTwoName), Is.EqualTo("value-two"));

            document.SetProperty(PropertyTwoName, "some-other-value");

            Assert.That(document.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(document.GetPropertyValue(PropertyTwoName), Is.EqualTo("some-other-value"));

            document.RemoveProperty(PropertyTwoName);

            Assert.That(document.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(() => document.GetPropertyValue(PropertyTwoName), Throws.InvalidOperationException);
        }

        [Test]
        public void Should_be_able_to_progress_to_cleared()
        {
            var document = GetDocument(Guid.NewGuid())
                .Processing()
                .Cleared();

            AssertDocumentCleared(document);
        }

        [Test]
        public void Should_be_able_to_progress_to_suspicious()
        {
            var document = GetDocument(Guid.NewGuid())
                .Processing()
                .Suspicious();

            AssertDocumentSuspicious(document);
        }

        [Test]
        public void Should_be_able_to_progress_to_suspicious_but_sanitized()
        {
            var document = GetDocument(Guid.NewGuid())
                .Processing()
                .WithSanitizedContent(_content)
                .Suspicious();

            AssertDocumentSuspiciousButSanitized(document);
        }
    }
}