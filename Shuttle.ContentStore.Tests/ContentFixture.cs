using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests
{
    [TestFixture]
    public class ContentFixture
    {
        private readonly byte[] _bytes = {0, 1, 2, 3};
        private readonly Dictionary<Guid, DateTime> _effectiveFromDates =new Dictionary<Guid, DateTime>();
        private const string FileName = "file-name";
        private const string ContentType = "content-type";
        private const string SystemName = "system-name";
        private const string Username = "username";
        private const string PropertyOneName = "property-one";
        private const string PropertyTwoName = "property-two";

        public Content GetContent(Guid id)
        {
            return GetContent(id, id);
        }

        public Content GetContent(Guid id, Guid referenceId)
        {
            _effectiveFromDates.Add(id, DateTime.Now);

            return new Content(id, referenceId, FileName, ContentType, _bytes, SystemName, Username, _effectiveFromDates[id]);
        }

        public void AssertContent(Content content, Guid id)
        {
            Assert.That(content.Id, Is.EqualTo(id));
            Assert.That(content.ReferenceId, Is.EqualTo(id));
            Assert.That(content.FileName, Is.EqualTo(FileName));
            Assert.That(content.ContentType, Is.EqualTo(ContentType));
            Assert.That(content.Bytes, Is.Not.Null);
            Assert.That(content.Bytes, Is.EquivalentTo(_bytes));
            Assert.That(content.SystemName, Is.EqualTo(SystemName));
            Assert.That(content.Username, Is.EqualTo(Username));
            Assert.That(content.Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(content.StatusDateRegistered, Is.EqualTo(_effectiveFromDates[id]));
            Assert.That(content.EffectiveFromDate, Is.EqualTo(_effectiveFromDates[id]));
            Assert.That(content.EffectiveToDate, Is.EqualTo(DateTime.MaxValue));

            var statusEvents = content.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(1));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[id]));
        }

        public void AssertContentPassed(Content content)
        {
            var statusEvents = content.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(3));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[content.Id]));
            Assert.That(statusEvents[1].Status, Is.EqualTo(ServiceStatus.Processing));
            Assert.That(statusEvents[2].Status, Is.EqualTo(ServiceStatus.Passed));

            Assert.That(content.HasSanitizedBytes, Is.False);
            Assert.That(() => content.SanitizedBytes, Throws.InvalidOperationException);
        }

        public void AssertContentSuspiciousButSanitized(Content content)
        {
            var statusEvents = content.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(3));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[content.Id]));
            Assert.That(statusEvents[1].Status, Is.EqualTo(ServiceStatus.Processing));
            Assert.That(statusEvents[2].Status, Is.EqualTo(ServiceStatus.Suspicious));

            Assert.That(content.HasSanitizedBytes, Is.True);
            Assert.That(content.SanitizedBytes, Is.EquivalentTo(_bytes));
        }

        public void AssertContentSuspicious(Content content)
        {
            var statusEvents = content.GetStatusEvents().ToList();

            Assert.That(statusEvents.Count, Is.EqualTo(3));
            Assert.That(statusEvents[0].Status, Is.EqualTo(ServiceStatus.Registered));
            Assert.That(statusEvents[0].DateRegistered, Is.EqualTo(_effectiveFromDates[content.Id]));
            Assert.That(statusEvents[1].Status, Is.EqualTo(ServiceStatus.Processing));
            Assert.That(statusEvents[2].Status, Is.EqualTo(ServiceStatus.Suspicious));

            Assert.That(content.HasSanitizedBytes, Is.False);
            Assert.That(() => content.SanitizedBytes, Throws.InvalidOperationException);
        }

        [Test]
        public void Should_be_able_to_change_status_information_on_status_event()
        {
            var content = GetContent(Guid.NewGuid());

            Assert.That(content.StatusDateRegistered, Is.EqualTo(_effectiveFromDates[content.Id]));

            var dateRegistered = DateTime.Now.AddSeconds(-5);

            content.OnStatusEvent(ServiceStatus.Passed, dateRegistered);

            Assert.That(content.StatusDateRegistered, Is.Not.EqualTo(_effectiveFromDates[content.Id]));
            Assert.That(content.StatusDateRegistered, Is.EqualTo(dateRegistered));
        }

        [Test]
        public void Should_be_able_to_instantiate_valid_content()
        {
            var id = Guid.NewGuid();
            var content = GetContent(id);

            AssertContent(content, id);
        }

        [Test]
        public void Should_be_able_to_manage_a_property()
        {
            var content = GetContent(Guid.NewGuid());

            Assert.That(content.GetProperties().Count, Is.Zero);
            Assert.That(() => content.GetPropertyValue(PropertyOneName), Throws.InvalidOperationException);
            Assert.That(() => content.GetPropertyValue(PropertyTwoName), Throws.InvalidOperationException);

            content.SetProperty(PropertyOneName, "value-one");

            Assert.That(content.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(() => content.GetPropertyValue(PropertyTwoName), Throws.InvalidOperationException);

            content.SetProperty(PropertyTwoName, "value-two");

            Assert.That(content.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(content.GetPropertyValue(PropertyTwoName), Is.EqualTo("value-two"));

            content.SetProperty(PropertyTwoName, "some-other-value");

            Assert.That(content.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(content.GetPropertyValue(PropertyTwoName), Is.EqualTo("some-other-value"));

            content.RemoveProperty(PropertyTwoName);

            Assert.That(content.GetPropertyValue(PropertyOneName), Is.EqualTo("value-one"));
            Assert.That(() => content.GetPropertyValue(PropertyTwoName), Throws.InvalidOperationException);
        }

        [Test]
        public void Should_be_able_to_progress_to_passed()
        {
            var content = GetContent(Guid.NewGuid())
                .Processing()
                .Passed();

            AssertContentPassed(content);
        }

        [Test]
        public void Should_be_able_to_progress_to_suspicious()
        {
            var content = GetContent(Guid.NewGuid())
                .Processing()
                .Suspicious();

            AssertContentSuspicious(content);
        }

        [Test]
        public void Should_be_able_to_progress_to_suspicious_but_sanitized()
        {
            var content = GetContent(Guid.NewGuid())
                .Processing()
                .WithSanitizedContent(_bytes)
                .Suspicious();

            AssertContentSuspiciousButSanitized(content);
        }
    }
}