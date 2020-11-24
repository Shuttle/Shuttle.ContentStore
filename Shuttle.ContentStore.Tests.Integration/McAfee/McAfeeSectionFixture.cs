using System;
using System.IO;
using Shuttle.ContentStore.McAfee;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Tests.Integration.McAfee
{
    [TestFixture]
    public class McAfeeSectionFixture
    {
        [Test]
        public void Should_be_able_to_load_the_configuration()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".config\\McAfee.config");
            var section = ConfigurationSectionProvider.OpenFile<McAfeeSection>("shuttle", "mcafee", path);

            Assert.That(section, Is.Not.Null);
            Assert.That(section.Url, Is.EqualTo("api-url"));
            Assert.That(section.Username, Is.EqualTo("api-username"));
            Assert.That(section.Password, Is.EqualTo("api-password"));
            Assert.That(section.SessionDuration, Is.EqualTo("00:00:05"));
            Assert.That(section.AnalyzerProfile, Is.EqualTo("analyzer-profile"));
            Assert.That(section.AcceptableSeverity, Is.EqualTo(3));
            Assert.That(section.HeartbeatInterval, Is.EqualTo("00:00:10"));
            Assert.That(section.PollScanInterval, Is.EqualTo("00:00:15"));

            var configuration = McAfeeSection.Configuration(path);

            Assert.That(configuration.Url, Is.EqualTo("api-url"));
            Assert.That(configuration.Username, Is.EqualTo("api-username"));
            Assert.That(configuration.Password, Is.EqualTo("api-password"));
            Assert.That(configuration.SessionDuration, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(configuration.AnalyzerProfile, Is.EqualTo("analyzer-profile"));
            Assert.That(configuration.AcceptableSeverity, Is.EqualTo(3));
            Assert.That(configuration.HeartbeatInterval, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(configuration.PollScanInterval, Is.EqualTo(TimeSpan.FromSeconds(15)));
        }
    }
}